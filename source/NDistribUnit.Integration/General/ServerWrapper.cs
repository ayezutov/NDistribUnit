using System;
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
            return Retry.While(
                () =>
                    {
                        AgentInformation found = ServerHost.ConnectionsTracker.Agents.FirstOrDefault(a => a.Endpoint.Address.Equals(agent.AgentHost.Endpoint.Address));
                        return found != null && found.State != AgentState.Disconnected;
                    }, 500);
        }

        public bool HasADisconnected(AgentWrapper agent)
        {
            return Retry.While(
                () =>
                    {
                        AgentInformation found = ServerHost.ConnectionsTracker.Agents.FirstOrDefault(
                            a => a.Endpoint.Address.Equals(agent.AgentHost.Endpoint.Address));
                        return found == null || found.State == AgentState.Disconnected;
                    }, 500);
       
        }

        public void ShutDownInExpectedWay()
        {
            ServerHost.Close();
        }

        public void ShutDownUngraceful()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            ServerHost.Abort();
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