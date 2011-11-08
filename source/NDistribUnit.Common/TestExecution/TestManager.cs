using System;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Server.Communication;
using NDistribUnit.Common.TestExecution.Data;
using NDistribUnit.Common.TestExecution.Storage;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
	public class TestManager: IAgentDataSource
	{
		private readonly ServerConnectionsTracker agents;
		private readonly TestUnitCollection tests;
	    private readonly IRequestsStorage requests;
        private readonly IProjectsStorage projects;
        private readonly ILog log;
        private ITestsRetriever testsRetriever;
        private readonly ITestsScheduler scheduler;
        private readonly IConnectionProvider connectionProvider;

        /// <summary>
		/// 
		/// </summary>
        private readonly object syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="TestManager"/> class.
        /// </summary>
        /// <param name="agents">The agents.</param>
        /// <param name="tests">The tests.</param>
        /// <param name="requests">The requests.</param>
        /// <param name="projects">The projects.</param>
        /// <param name="log">The log.</param>
        /// <param name="testsRetriever">The tests parser.</param>
        /// <param name="scheduler">The scheduler.</param>
        /// <param name="connectionProvider">The connection provider.</param>
	    public TestManager(ServerConnectionsTracker agents, 
            TestUnitCollection tests, 
            IRequestsStorage requests,
            IProjectsStorage projects,
            ILog log, 
            ITestsRetriever testsRetriever, 
            ITestsScheduler scheduler, 
            IConnectionProvider connectionProvider)
		{
			this.agents = agents;
			agents.AgentFreed += OnAgentFreed;
            agents.AgentDisconnected += OnAgentDisconnected;

			this.tests = tests;
            tests.AvailableAdded += OnAvailableTestAdded;

            this.requests = requests;
            requests.Added += OnRequestsAdded;

            this.projects = projects;
            this.log = log;
            this.testsRetriever = testsRetriever;
            this.scheduler = scheduler;
            this.connectionProvider = connectionProvider;
		}

        private void OnAgentDisconnected(object sender, EventArgs<AgentInformation> e)
        {
            lock (syncRoot)
            {
                // 1. remove related test from running and add to available
                // 2. recheck, whether under these conditions a test should be run on a not preferrable agent.
            }
        }

        private void OnAvailableTestAdded(object sender, EventArgs<TestUnit> e)
        {
            lock (syncRoot)
            {
                TestUnit test = e.Data;
                var agent = scheduler.GetAgentForTest(test);
                if (agent == null)
                    return;

                Run(test, agent);
            }
        }

        private void OnAgentFreed(object sender, EventArgs<AgentInformation> e)
        {
            lock(syncRoot)
            {
                AgentInformation agent = e.Data;
                var test = scheduler.GetTestForAgent(agent);

                if (test == null)
                    return;

                Run(test, agent);
            }
        }

        private void Run(TestUnit test, AgentInformation agent)
        {
            tests.MarkRunning(test);

            test.Request.Status = TestRunRequestStatus.Pending;

            agents.MarkBusy(agent);
            new Action(() =>
                           {
                               try
                               {
                                   var result = connectionProvider.GetDuplexConnection<ITestRunnerAgent, IAgentDataSource>(this, agent.Endpoint.Address)
                                       .RunTests(test);

                                   if (result.IsFailure)
                                   {
                                       var handling = test.Request.TestRun.Parameters.GetSpecialHandling(result.Exception);
                                       if (handling != null)
                                       {
                                           scheduler.ProcessSpecialHandling(test, agent, handling);
                                       }
                                   }

                                   test.Results.Add(result);
                                   tests.MarkCompleted(test);
                               }
                               catch (Exception ex)
                               {
                                   log.Error("Exception while running test", ex);
                                   tests.Add(test);
                               }
                           }).BeginInvoke(null, null);
            
        }

        private void OnRequestsAdded(object sender, EventArgs<TestRunRequest> args)
        {
            var request = args.Data;
            var project = projects.GetProject(request.TestRun);

            if (project == null)
            {
                try
                {
                    var packedProject = request.Client.GetPackedProject(request.TestRun.Id);
                    project = projects.Store(request.TestRun, packedProject);
                }
                catch (Exception ex)
                {
                    log.Error("Unable to get test project", ex);
                    // TODO: add retrying here
                    RemoveAndMarkAsInvalid(request);
                    return;
                }
            }

            if (project == null)
            {
                RemoveAndMarkAsInvalid(request);
                return;
            }

            try
            {
                tests.AddRange(testsRetriever.Get(project, request));
            }
            catch(Exception ex)
            {
                log.Error("Error while parsing test request", ex);
                RemoveAndMarkAsInvalid(request);
            }
        }

        private void RemoveAndMarkAsInvalid(TestRunRequest request)
        {
            requests.Remove(request);
            request.Client.NotifyTestProgressChanged(new TestResult("Invalid"), true);
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