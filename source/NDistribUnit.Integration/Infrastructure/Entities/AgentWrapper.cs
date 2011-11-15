using System;
using NDistribUnit.Common.Agent;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Extensions;
using NDistribUnit.Integration.Tests.Infrastructure.Stubs;

namespace NDistribUnit.Integration.Tests.Infrastructure.Entities
{
    public class AgentWrapper: IDisposable
    {
        private readonly IVersionProvider versionProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentWrapper"/> class.
        /// </summary>
        /// <param name="agentHost">The agent host.</param>
        /// <param name="testRunner">The test runner.</param>
        /// <param name="updateReceiver">The update receiver.</param>
        /// <param name="versionProvider">The version provider.</param>
        public AgentWrapper(AgentHost agentHost, Common.Agent.Agent testRunner, TestUpdateReceiver updateReceiver, IVersionProvider versionProvider)
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

        public TestUpdateReceiver UpdateReceiver { get; set; }

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

        public Version GetVersion()
        {
            return versionProvider.GetVersion();
        }
    }
}