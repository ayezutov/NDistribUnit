using System;
using System.Collections.Generic;
using System.ServiceModel;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.TestExecution.Data;
using NDistribUnit.Common.TestExecution.Exceptions;
using NDistribUnit.Common.TestExecution.Storage;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public class ServerTestRunner : IAgentDataSource
    {
        private readonly TestAgentsCollection agents;
        private readonly TestUnitsCollection tests;

        private readonly IRequestsStorage requests;
        private readonly IProjectsStorage projects;
        private readonly IResultsStorage results;
        private readonly ILog log;
        private readonly ITestsRetriever testsRetriever;
        private readonly ITestsScheduler scheduler;
        private readonly IReprocessor reprocessor;
        private readonly IConnectionProvider connectionProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerTestRunner"/> class.
        /// </summary>
        /// <param name="agents">The agents.</param>
        /// <param name="tests">The tests.</param>
        /// <param name="requests">The requests.</param>
        /// <param name="projects">The projects.</param>
        /// <param name="results">The results.</param>
        /// <param name="log">The log.</param>
        /// <param name="testsRetriever">The tests parser.</param>
        /// <param name="scheduler">The scheduler.</param>
        /// <param name="reprocessor">The reprocessor.</param>
        /// <param name="connectionProvider">The connection provider.</param>
        public ServerTestRunner(TestAgentsCollection agents,
                                TestUnitsCollection tests,
                                IRequestsStorage requests,
                                IProjectsStorage projects,
                                IResultsStorage results,
                                ILog log,
                                ITestsRetriever testsRetriever,
                                ITestsScheduler scheduler,
                                IReprocessor reprocessor,
                                IConnectionProvider connectionProvider)
        {
            // Initializing fields
            this.projects = projects;
            this.results = results;
            this.log = log;
            this.testsRetriever = testsRetriever;
            this.scheduler = scheduler;
            this.reprocessor = reprocessor;
            this.connectionProvider = connectionProvider;
            this.requests = requests;
            this.agents = agents;
            this.tests = tests;

            // Binding to request collection events
            requests.Added += (sender, args) => RunAsynchronously(() => ProcessRequest(args.Data));

            // Binding to agent collection events
            agents.AgentFreed += (sender, args) => RunAsynchronously(TryToRunIfAvailable);
            agents.AgentDisconnected += (sender, args) => RunAsynchronously(TryToRunIfAvailable);

            // Binding to test collection events
            tests.AvailableAdded += (sender, args) => RunAsynchronously(TryToRunIfAvailable); 
        }

        private void RunAsynchronously(Action action)
        {
            try
            {
                action.BeginInvoke(null, null);
            }
            catch (Exception ex)
            {
                log.Error("Exception while running an action in parallel", ex);
            }
        }

        // Should be started asynchronously to avoid any deadlocks
        private void TryToRunIfAvailable()
        {
            Tuple<AgentInformation, TestUnitWithMetadata> pair = null;
            // lock both collections
            // TODO: check the flows for no deadlocks
            lock (agents.SyncObject)
            {
                lock (tests.SyncObject)
                {
                    try
                    {
                        pair = scheduler.GetAgentAndTest();
                    }
                    catch(NoAvailableAgentsException ex)
                    {
                        MarkAsInvalidAndComplete(ex.Tests);
                        return;
                    }

                    if (pair == null)
                        return;

                    tests.MarkRunning(pair.Item2);
                    agents.MarkBusy(pair.Item1);
                }
            }

            Run(pair.Item2, pair.Item1);
        }

        // Should be started in a separate thread
        private void Run(TestUnitWithMetadata test, AgentInformation agent)
        {
            var request = requests.GetBy(test.Test.Run);
            if (request != null)
                request.Status = TestRunRequestStatus.Pending;
            TestResult result = null;
            try
            {
                var testRunnerAgent =
                    connectionProvider.GetDuplexConnection<IAgent, IAgentDataSource>(this,
                                                                                     agent.Address);
                result = testRunnerAgent.RunTests(test.Test);
            }
            catch (CommunicationException ex)
            {
                log.Error("Exception while running test", ex);
                agent.State = AgentState.Ready;
                tests.Add(test);
            }
            catch (Exception ex)
            {
                log.Error("Exception while running test", ex);
                agent.State = AgentState.Error;
                tests.Add(test);
            }

            ProcessResult(test, agent, result);
        }

        private void ProcessResult(TestUnitWithMetadata test, AgentInformation agent, TestResult result)
        {
            bool isRequestCompleted;
            lock (agents.SyncObject)
            {
                lock (tests.SyncObject)
                {
                    test.Results.Add(result);
                    tests.MarkCompleted(test);
                    agents.MarkAsReady(agent);

                    reprocessor.AddForReprocessingIfRequired(test, result);

                    isRequestCompleted = !tests.IsAnyAvailableFor(test.Test.Run);
                }
            }

            results.Add(test, result);

            if (isRequestCompleted)
                ProcessCompletedTestRun(test);
        }

        private void ProcessCompletedTestRun(TestUnitWithMetadata test)
        {
            var request = requests.RemoveBy(test.Test.Run);

            var result = results.StoreAsCompleted(test.Test.Run);
            var client = request.Client;
            if (client == null)
                log.Warning(string.Format("Unable to notify the client about completed test run: {0}", test.Test.Run.Id));
            else
            {
                try
                {
                    client.NotifyTestProgressChanged(result, true);
                }
                catch(Exception ex)
                {
                    log.Warning(string.Format("Error while notifying the client about completed test run: {0}", test.Test.Run.Id),
                                ex);
                }
            }

        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="request">The request.</param>
        private void ProcessRequest(TestRunRequest request)
        {
            TestProject project;

            try
            {
                project = projects.GetOrLoad(request.TestRun,
                                         () => request.Client.GetPackedProject(request.TestRun.Id));
            }
            catch (Exception ex)
            {
                log.Error("Unable to get test project", ex);
                MarkAsInvalidAndComplete(request);
                return;
            }
            
            if (project == null)
            {
                MarkAsInvalidAndComplete(request);
                return;
            }

            try
            {
                log.BeginActivity("Parsing project into separate test units");
                tests.AddRange(testsRetriever.Get(project, request));
                log.EndActivity("Finished parsing project into separate test units");
            }
            catch (Exception ex)
            {
                log.Error("Error while parsing test request", ex);
                MarkAsInvalidAndComplete(request);
            }
        }

        private void MarkAsInvalidAndComplete(IEnumerable<TestRunRequest> requests)
        {
            foreach (var request in requests)
            {
                MarkAsInvalidAndComplete(request);
            }
        }

        private void MarkAsInvalidAndComplete(TestRunRequest request)
        {
            requests.Remove(request);
            try
            {
                request.Client.NotifyTestProgressChanged(new TestResult( /*mark invalid*/), true);
            }
            catch(Exception ex)
            {
                log.Warning("Unable to report test result", ex);
            }
        }

        /// <summary>
        /// Gets the packed project.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <returns></returns>
        public PackedProject GetPackedProject(TestRun testRun)
        {
            return projects.GetPackedProject(testRun);
        }
    }
}