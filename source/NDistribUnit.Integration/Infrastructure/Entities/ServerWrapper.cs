using System;
using System.ServiceModel;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Retrying;
using NDistribUnit.Common.Server.Communication;
using System.Linq;
using NDistribUnit.Common.Server.Services;
using NDistribUnit.Common.TestExecution;

namespace NDistribUnit.Integration.Tests.Infrastructure.Entities
{
    /// <summary>
    /// The class is a special wrapper around real server program to allow easy access
    /// in testing code
    /// </summary>
    public class ServerWrapper: IDisposable
    {
        private readonly ServerConnectionsTracker serverConnectionsTracker;
        private readonly TestAgentsCollection agents;
        private ServerHost ServerHost { get; set; }

        public TestRunnerServer TestRunner { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerWrapper"/> class.
        /// </summary>
        /// <param name="testRunner">The test runner.</param>
        /// <param name="serverConnectionsTracker">The server connections tracker.</param>
        /// <param name="agents">The agents.</param>
        /// <param name="serverHost">The server host.</param>
        public ServerWrapper(
            TestRunnerServer testRunner, 
            ServerConnectionsTracker serverConnectionsTracker, 
            TestAgentsCollection agents,
            ServerHost serverHost = null)
        {
            TestRunner = testRunner;
            this.serverConnectionsTracker = serverConnectionsTracker;
            this.agents = agents;
            ServerHost = serverHost;
        }

        public void Start()
        {
            if (ServerHost != null)
                ServerHost.Start();
            else
                serverConnectionsTracker.Start();
        }

        /// <summary>
        /// Determines whether the server has a connected agent with the specified name.
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <returns>
        ///   <c>true</c> if the server has a connected agent with the specified name; otherwise, <c>false</c>.
        /// </returns>
        public bool HasAConnected(AgentWrapper agent)
        {
            string agentName = agent.TestRunner.Name;

            return HasAConnected(agentName);
        }

        /// <summary>
        /// Determines whether the server has a connected agent with the specified name.
        /// </summary>
        /// <param name="agentName">The agent.</param>
        /// <param name="address">The address.</param>
        /// <returns>
        ///   <c>true</c> if the server has a connected agent with the specified name; otherwise, <c>false</c>.
        /// </returns>
        public bool HasAConnected(string agentName, EndpointAddress address = null)
        {
            return Retry.While(
                 () =>
                 {
                     AgentInformation found = agents.GetBy(agentName, address);
                     return found != null && found.State != AgentState.Disconnected;
                 }, 500);
        }

        public bool HasADisconnected(AgentWrapper agent)
        {
            string agentName = agent.TestRunner.Name;
            return HasADisconnected(agentName);
        }

        /// <summary>
        /// Determines whether the server has a disconnected agent with the specified name.
        /// </summary>
        /// <param name="agentName">Name of the agent.</param>
        /// <returns>
        ///   <c>true</c> if the server has a disconnected agent with the specified name; otherwise, <c>false</c>.
        /// </returns>
        public bool HasADisconnected(string agentName)
        {
            return Retry.While(
                () =>
                {
                    AgentInformation found = agents.GetBy(agentName);
                    return found == null || found.State == AgentState.Disconnected;
                }, 500);
        }

        private void ShutDownUngraceful()
        {
            if (ServerHost != null)
                ServerHost.Abort();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            ShutDownUngraceful();
        }

        /// <summary>
        /// Determines whether the server has no conected agents.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the server has no connected agents; otherwise, <c>false</c>.
        /// </returns>
        public bool HasNoConnectedAgents()
        {
            return !Retry.While(() => agents.Count != 0, 500);
        }
    }
}