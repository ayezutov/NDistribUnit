using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Common.ConsoleProcessing;
using NDistribUnit.Common.Common.Extensions;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.DataContracts;
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
        private readonly ExceptionCatcher exceptionCatcher;
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
        /// <param name="exceptionCatcher">The exception catcher.</param>
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
                                ExceptionCatcher exceptionCatcher,
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
            this.exceptionCatcher = exceptionCatcher;
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
            
            Action wrappedAction = () => exceptionCatcher.Run(action);
            exceptionCatcher.Run(()=>wrappedAction.BeginInvoke(null, null));
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
                            log.BeginActivity("Tests are being marked as not run due to not available agents...");
                            foreach (var test in ex.Tests)
                            {
                                ProcessResult(test, null,
                                              TestResultFactory.GetNoAvailableAgentsFailure(test, ex));
                            }
                            log.EndActivity("Completed marking tests as not run due to not available agents");
                            return;
                        }

                        if (pair == null)
                            return;

                    }
                    catch(Exception ex)
                    {
                        log.Error("Exception while running test", ex);
                        throw;
                    }

                    tests.MarkRunning(pair.Item2);
                    agents.MarkAsBusy(pair.Item1);

                    if (tests.HasAvailable)
                        RunAsynchronously(TryToRunIfAvailable);
                }
            }

            Run(pair.Item2, pair.Item1, pair.Item3);
        }

        // Should be started in a separate thread
        private void Run(TestUnitWithMetadata test, AgentMetadata agent, DistributedConfigurationSubstitutions configurationSubstitutions)
        {
            log.BeginActivity(string.Format("[{0}] Started test {1}", test.Test.Run, test.Test.Info.TestName.FullName));
            var request = requests.GetBy(test.Test.Run);
            if (request != null)
                request.Status = TestRunRequestStatus.Pending;
            try
            {
                log.BeginActivity(string.Format("Connecting to agent [{0}]...", agent));
                var testRunnerAgent = connectionProvider.GetConnection<IAgent>(agent.Address);
                log.BeginActivity(string.Format("Connected to agent [{0}]", agent));

                log.BeginActivity(string.Format("Checking project existence ('{0}') on agent {1}", test.Test.Run, agent));
                if (!testRunnerAgent.HasProject(test.Test.Run))
                {
                    log.EndActivity(string.Format("Project '{0}' doesn't exist on agent {1}", test.Test.Run, agent));

                    log.BeginActivity(string.Format("Sending project ('{0}') to agent {1}", test.Test.Run, agent));
                    testRunnerAgent.ReceiveProject(new ProjectMessage
                                                       {
                                                           TestRun = test.Test.Run,
                                                           Project = projects.GetStreamToPacked(test.Test.Run) ?? new MemoryStream(1)
                                                       });
                    log.EndActivity(string.Format("Sent project ('{0}') to agent {1}", test.Test.Run, agent));
                }
                else
                    log.EndActivity(string.Format("Project '{0}' exist on agent {1}", test.Test.Run, agent));

                log.BeginActivity(string.Format("Running {0} on {1} with variables ({2})...", test.Test.UniqueTestId, agent, configurationSubstitutions));
                TestResult result = testRunnerAgent.RunTests(test.Test, configurationSubstitutions);
                log.EndActivity(string.Format("Finished running {0} on {1}...", test.Test.UniqueTestId, agent));

                if (result == null)
                    throw new FaultException("Result is not available");

                ProcessResult(test, agent, result);
            }
            catch (FaultException ex)
            {
                log.Error(string.Format("Exception while running test {0} on {1}", test.Test.UniqueTestId, agent), ex);
                agents.MarkAsFailure(agent);
                tests.Add(test);
            }
            catch (CommunicationException ex)
            {
                log.Error(string.Format("Exception in communication while running test {0} on {1}", test.Test.UniqueTestId, agent), ex);
                agents.MarkAsDisconnected(agent);
                tests.Add(test);
            }
            catch(Exception ex)
            {
                log.Error(string.Format("Something bad and unhandled happened while running test {0} on {1}", test.Test.UniqueTestId, agent), ex);
            }

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
            var request = requests.GetBy(test.Test.Run);
            if (request != null)
                request.PipeToClient.Publish(result.DeepClone().SetFinal(false));

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
                var foundChildResult = FindByName(found, child.Test.UniqueTestId);
                if (foundChildResult == null)
                    continue;

                child.Results.Add(foundChildResult);
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
                log.Error("Unable to get test project");
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
                log.Info(string.Format("There were {0} tests found in {1}", testUnits != null ? testUnits.Count : 0, request.TestRun));
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
            log.EndActivity(string.Format("Finished running request: {0}", testRun));
            log.BeginActivity("Performing cleanup operations on server and agents...");
            var request = requests.GetBy(testRun);

            log.BeginActivity(string.Format("Storing test results for request {0}", testRun));
            var result = results.StoreAsCompleted(testRun);
            log.EndActivity(string.Format("Completed storing test results for request {0}", testRun));

            request.PipeToClient.Publish(result.SetFinal());
            request.PipeToClient.Close();

            log.BeginActivity("Releasing resources on agents...");
            var agentsToFree = agents.GetAgents(a => a.Status.IsOneOf(
                AgentState.Ready,
                AgentState.Busy,
                AgentState.Error,
                AgentState.Updating));
            foreach (var agentToFree in agentsToFree)
            {
                var agentConnection = connectionProvider.GetConnection<IAgent>(agentToFree.Address);
                agentConnection.ReleaseResources(testRun);
            }

            log.EndActivity("Finished releasing resources on agents...");

            log.BeginActivity("Releasing resources on server...");
            if (!testRun.IsAliasedTest)
                projects.RemoveProject(testRun);
            log.EndActivity("Finished releasing resources on server...");



            log.EndActivity(string.Format("Cleanup operations were completed for request {0}", testRun));
            
        }
    }
}