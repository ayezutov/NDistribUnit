using System;
using System.ServiceModel;
using System.Threading;
using NDistribUnit.Common.Agent;
using NDistribUnit.Common.Common.Logging;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Extensions;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.TestExecution.DistributedConfiguration;
using NDistribUnit.Integration.Tests.Infrastructure.Stubs;
using NUnit.Core;

namespace NDistribUnit.Integration.Tests.Infrastructure.Entities
{
    public class AgentWrapper: IDisposable, IRemoteAppPart, IAgent
    {
        private readonly TestingVersionProvider versionProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentWrapper"/> class.
        /// </summary>
        /// <param name="agentHost">The agent host.</param>
        /// <param name="testRunner">The test runner.</param>
        /// <param name="updateReceiver">The update receiver.</param>
        /// <param name="versionProvider">The version provider.</param>
        public AgentWrapper(AgentHost agentHost, Common.Agent.Agent testRunner, TestUpdateReceiver updateReceiver, TestingVersionProvider versionProvider)
        {
            this.versionProvider = versionProvider;
            AgentHost = agentHost;
            UpdateReceiver = updateReceiver;
            TestRunner = agentHost != null ? agentHost.TestRunner : testRunner;
        }

        /// <summary>
        /// Gets the agent host.
        /// </summary>
        public AgentHost AgentHost { get; private set; }

        public TestUpdateReceiver UpdateReceiver { get; private set; }

        /// <summary>
        /// Gets the test runner.
        /// </summary>
        public Common.Agent.Agent TestRunner { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is started.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is started; otherwise, <c>false</c>.
        /// </value>
        public bool IsStarted { get; private set; }

        /// <summary>
        /// Occurs when this agent is started.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Occurs when this agent is stopped.
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            if (AgentHost != null)
                AgentHost.Start();
            IsStarted = true;
            Started.SafeInvoke(this);
        }

        /// <summary>
        /// Shuts down in expected way.
        /// </summary>
        public void ShutDownInExpectedWay()
        {
            if (AgentHost != null)
                AgentHost.Stop();
            IsStarted = false;
            Stopped.SafeInvoke(this);
        }

        /// <summary>
        /// Shuts down ungraceful.
        /// </summary>
        public void ShutDownUngraceful()
        {
            IsStarted = false;
            if (AgentHost != null)
                AgentHost.Abort();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            if (AgentHost != null)
                AgentHost.Abort();
        }

        /// <summary>
        /// Changes the name to.
        /// </summary>
        /// <param name="newName">The new name.</param>
        public void ChangeNameTo(string newName)
        {
            var oldName = TestRunner.Name;


            if (AgentHost == null && !newName.Equals(oldName))
                Stopped.SafeInvoke(this);

            TestRunner.Name = newName;

            if (AgentHost == null && !newName.Equals(oldName))
                Started.SafeInvoke(this);
        }


        /// <summary>
        /// Pings the tracking side
        /// </summary>
        /// <param name="pingInterval"></param>
        /// <returns>Anything (including null) if everything is ok, throws exception otherwise</returns>
        public PingResult Ping(TimeSpan pingInterval)
        {
            if (IsStarted)
                return TestRunner.Ping(pingInterval);

            throw new CommunicationException("Agent seems to be not available");
        }

        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <param name="maxItemsCount">The max items count.</param>
        /// <param name="lastFetchedEntryId">The last fetched entry id.</param>
        /// <returns></returns>
        public LogEntry[] GetLog(int maxItemsCount, int? lastFetchedEntryId)
        {
            if (IsStarted)
                return TestRunner.GetLog(maxItemsCount, lastFetchedEntryId);

            throw new CommunicationException("Agent seems to be not available");
        }

        /// <summary>
        /// Receives the update pakage.
        /// </summary>
        /// <param name="updatePackage"></param>
        public void ReceiveUpdatePackage(UpdatePackage updatePackage)
        {
            if (IsStarted)
            {
                TestRunner.ReceiveUpdatePackage(updatePackage);

                // emulate, that an updates monitor has detected an update
                // and triggered system restart
                new Action(() =>
                               {
                                   Thread.Sleep(50);
                                   ShutDownInExpectedWay();
                                   versionProvider.SetVersion(updatePackage.Version);
                                   Start();
                               }).BeginInvoke(null, null);
                return;
            }

            throw new CommunicationException("Agent seems to be not available");
        }

        /// <summary>
        /// Runs tests on agent
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="configurationSubstitutions">The configuration substitutions.</param>
        /// <returns></returns>
        public TestResult RunTests(TestUnit test, DistributedConfigurationSubstitutions configurationSubstitutions)
        {
            if (IsStarted)
                return TestRunner.RunTests(test, configurationSubstitutions);

            throw new CommunicationException("Agent seems to be not available");
        }

        public void ReleaseResources(TestRun testRun)
        {
            if (IsStarted)
                TestRunner.ReleaseResources(testRun);
            else
                throw new CommunicationException("Agent seems to be not available");
        }

        /// <summary>
        /// Receives the project.
        /// </summary>
        /// <param name="project">The project.</param>
        public void ReceiveProject(ProjectMessage project)
        {
            if (IsStarted)
                TestRunner.ReceiveProject(project);
            else
                throw new CommunicationException("Agent seems to be not available");
        }

        /// <summary>
        /// Receives the project.
        /// </summary>
        /// <param name="run">The run.</param>
        /// <returns>
        ///   <c>true</c> if the specified agent has a project for the given run; otherwise, <c>false</c>.
        /// </returns>
        public bool HasProject(TestRun run)
        {
            if (IsStarted)
                return TestRunner.HasProject(run);

            throw new CommunicationException("Agent seems to be not available");
        }
    }
}