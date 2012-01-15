using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Server.AgentsTracking;
using NDistribUnit.Common.TestExecution.Data;
using NDistribUnit.Common.TestExecution.DistributedConfiguration;
using NDistribUnit.Common.TestExecution.Exceptions;
using NDistribUnit.Common.TestExecution.Storage;
using NUnit.Core;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public class ServerTestRunner
    {
        private readonly AgentsCollection agents;
        private readonly TestUnitsCollection tests;

        private readonly IRequestsStorage requests;
        private readonly IProjectsStorage projects;
        private readonly IResultsStorage results;
        private readonly ILog log;
        private readonly ITestsRetriever testsRetriever;
        private readonly IDistributedConfigurationOperator configurationOperator;
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
        /// <param name="configurationOperator">The configuration reader.</param>
        /// <param name="scheduler">The scheduler.</param>
        /// <param name="reprocessor">The reprocessor.</param>
        /// <param name="connectionProvider">The connection provider.</param>
        public ServerTestRunner(AgentsCollection agents,
                                TestUnitsCollection tests,
                                IRequestsStorage requests,
                                IProjectsStorage projects,
                                IResultsStorage results,
                                ILog log,
                                ITestsRetriever testsRetriever,
                                IDistributedConfigurationOperator configurationOperator,
                                ITestsScheduler scheduler,
                                IReprocessor reprocessor,
                                IConnectionProvider connectionProvider)
        {
            // Initializing fields
            this.projects = projects;
            this.results = results;
            this.log = log;
            this.testsRetriever = testsRetriever;
            this.configurationOperator = configurationOperator;
            this.scheduler = scheduler;
            this.reprocessor = reprocessor;
            this.connectionProvider = connectionProvider;
            this.requests = requests;
            this.agents = agents;
            this.tests = tests;

            // Binding to request collection events
            requests.Added += (sender, args) => RunAsynchronously(() => ProcessRequest(args.Data));

            // Binding to agent collection events
            agents.ReadyAgentAppeared += (sender, args) => RunAsynchronously(TryToRunIfAvailable);
            agents.ClientDisconnectedOrFailed += (sender, args) => RunAsynchronously(TryToRunIfAvailable);

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
            Tuple<AgentMetadata, TestUnitWithMetadata, DistributedConfigurationSubstitutions> pair;
            // lock both collections
            using (agents.Lock())
            {
                lock (tests.SyncObject)
                {
                    try
                    {
                        try
                        {
                            pair = scheduler.GetAgentAndTestAndVariables();
                        }
                        catch (NoAvailableAgentsException ex)
                        {
                            foreach (var test in ex.Tests)
                            {
                                ProcessResult(test, null,
                                              TestResultFactory.GetNoAvailableAgentsFailure(test, ex));
                            }
                            return;
                        }

                        if (pair == null)
                            return;

                        tests.MarkRunning(pair.Item2);
                        agents.MarkAsBusy(pair.Item1);

                        if (tests.HasAvailable)
                            RunAsynchronously(TryToRunIfAvailable);
                    }
                    catch(Exception ex)
                    {
                        log.Error("Exception while running test", ex);
                        throw;
                    }
                }
            }

            Run(pair.Item2, pair.Item1, pair.Item3);
        }

        // Should be started in a separate thread
        private void Run(TestUnitWithMetadata test, AgentMetadata agent, DistributedConfigurationSubstitutions configurationSubstitutions)
        {
            var request = requests.GetBy(test.Test.Run);
            if (request != null)
                request.Status = TestRunRequestStatus.Pending;
            TestResult result = null;
            try
            {
                var testRunnerAgent =
                    connectionProvider.GetConnection<IAgent>(agent.Address);

                if (!testRunnerAgent.HasProject(test.Test.Run))
                {
                    testRunnerAgent.ReceiveProject(new ProjectMessage()
                                                       {
                                                           TestRun = test.Test.Run,
                                                           Project = projects.GetStreamToPacked(test.Test.Run) ?? new MemoryStream(1)
                                                       });
                }
                result = testRunnerAgent.RunTests(test.Test, configurationSubstitutions);
            }
            catch (CommunicationException ex)
            {
                log.Error("Exception while running test", ex);
                agents.MarkAsDisconnected(agent);
                tests.Add(test);
            }
            catch (Exception ex)
            {
                log.Error("Exception while running test", ex);
                agents.MarkAsFailure(agent);
                tests.Add(test);
            }

            ProcessResult(test, agent, result);
        }

        private void ProcessResult(TestUnitWithMetadata test, AgentMetadata agent, TestResult result)
        {
            bool isRequestCompleted;
            using (agents.Lock())
            {
                lock (tests.SyncObject)
                {
                    AddResultsToTestUnitMetadata(test, result);
                    tests.MarkCompleted(test);
                    agents.MarkAsReady(agent);

                    reprocessor.AddForReprocessingIfRequired(test, result);

                    isRequestCompleted = !tests.IsAnyAvailableFor(test.Test.Run);
                }
            }

            results.Add(result, test.Test.Run);

            if (isRequestCompleted)
                ProcessCompletedTestRun(test.Test.Run);
        }

        private static void AddResultsToTestUnitMetadata(TestUnitWithMetadata test, TestResult result)
        {
            var found = FindByName(result, test.Test.UniqueTestId);
            if (found == null)
                return;

            test.Results.Add(found);

            foreach (var child in test.Children)
            {
                var foundChild = FindByName(found, child.Test.UniqueTestId);
                if (foundChild == null)
                    continue;
            }
        }

        private static TestResult FindByName(TestResult result, string name)
        {
            if (result.FullName == name)
                return result;

            if (result.Results != null)
                return result.Results.Cast<TestResult>().Select(child => FindByName(child, name)).FirstOrDefault(foundChild => foundChild != null);

            return null;
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
                log.BeginActivity(string.Format("Starting executing project request {0}", request.TestRun.Id));
                project = projects.Get(request.TestRun);
            }
            catch (Exception ex)
            {
                log.Error("Unable to get test project", ex);
                Complete(TestResultFactory.GetProjectRetrievalFailure(request, ex), request.TestRun);
                return;
            }
            
            if (project == null)
            {
                Complete(TestResultFactory.GetProjectRetrievalFailure(request), request.TestRun);
                return;
            }

            try
            {
                log.BeginActivity(string.Format("Starting parsing project request into test units {0}", request.TestRun.Id));
                var testUnits = testsRetriever.Get(project, request);
                if (testUnits != null && testUnits.Count == 0)
                {
                    log.Warning(string.Format("No tests were found in request: {0}", request.TestRun.Id));
                    Complete(TestResultFactory.GetNoAvailableTestFailure(request), request.TestRun);
                    return;
                }

                request.ConfigurationSetup = configurationOperator.ReadConfigurationSetup(project, request.TestRun.NUnitParameters);
                tests.AddRange(testUnits);
                log.EndActivity("Finished parsing project into separate test units");
            }
            catch (Exception ex)
            {
                log.Error("Error while parsing test request", ex);
                Complete(TestResultFactory.GetUnhandledExceptionFailure(request, ex), request.TestRun);
            }
        }

        private void Complete(TestResult result, TestRun testRun)
        {
            results.Add(result, testRun);
            ProcessCompletedTestRun(testRun);
        }

        private void ProcessCompletedTestRun(TestRun testRun)
        {
            var request = requests.RemoveBy(testRun);
            var result = results.StoreAsCompleted(testRun);
            
            request.PipeToClient.Publish(result.SetFinal());
            request.PipeToClient.Close();

            log.EndActivity(string.Format("Finished running request: {0}", testRun.Id));
        }
    }
}