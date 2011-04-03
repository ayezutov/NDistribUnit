using System;
using System.ServiceModel;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Retrying;
using NDistribUnit.Server.Communication;
using System.Linq;

namespace NDistribUnit.Integration.Tests.General
{
    /// <summary>
    /// The class is a special wrapper around real server program to allow easy access
    /// in testing code
    /// </summary>
    public class ServerWrapper: IDisposable
    {
        private ServerHost ServerHost { get; set; }

        internal ServerWrapper(ServerHost serverHost)
        {
            ServerHost = serverHost;
        }

        public void Start()
        {
            ServerHost.Start();
        }

        public bool HasAConnected(AgentWrapper agent)
        {
            string agentName = agent.AgentHost.TestRunner.Name;

            return HasAConnected(agentName);
        }

        public bool HasAConnected(string agentName, EndpointAddress address = null)
        {
            return Retry.While(
                 () =>
                 {
                     AgentInformation found = ServerHost.ConnectionsTracker.Agents.FirstOrDefault(a => a.Name.Equals(agentName) && (address == null || a.Endpoint.Address.Equals(address)));
                     return found != null && found.State != AgentState.Disconnected;
                 }, 500);
        }

        public bool HasADisconnected(AgentWrapper agent)
        {
            string agentName = agent.AgentHost.TestRunner.Name;
            return HasADisconnected(agentName);
        }

        public bool HasADisconnected(string agentName)
        {
            return Retry.While(
                () =>
                {
                    AgentInformation found = ServerHost.ConnectionsTracker.Agents.FirstOrDefault(
                        a => a.Name.Equals(agentName));
                    return found == null || found.State == AgentState.Disconnected;
                }, 500);
        }

        public void ShutDownInExpectedWay()
        {
            ServerHost.Close();
        }

        public void ShutDownUngraceful()
        {
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
            return !Retry.While(() => ServerHost.ConnectionsTracker.Agents.Count != 0, 500);
        }
    }
}