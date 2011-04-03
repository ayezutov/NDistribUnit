using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.Communication.ConnectionTracking;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Server.Communication
{
    /// <summary>
    /// Tracks connections on NDistribUnit server. It always has a list of agents with actual statuses.
    /// </summary>
    public class ServerConnectionsTracker
    {
        private readonly ILog log;
        private readonly IConnectionsTracker<ITestRunnerAgent> connectionsTracker;

        /// <summary>
        /// Gets the list of agents
        /// </summary>
        public IList<AgentInformation> Agents { get; private set; }

        /// <summary>
        /// Initializes a new instance of a connection tracker
        /// </summary>
        /// <param name="connectionsTracker"></param>
        /// <param name="log"></param>
        public ServerConnectionsTracker(IConnectionsTracker<ITestRunnerAgent> connectionsTracker, ILog log)
        {
            this.log = log;
            Agents = new List<AgentInformation>();
            this.connectionsTracker = connectionsTracker;
            this.connectionsTracker.EndpointConnected += OnEndpointConnected;
            this.connectionsTracker.EndpointDisconnected += OnEndpointDisconnected;
            this.connectionsTracker.EndpointSuccessfulPing += OnEndpointSuccessfulPing;
        }

        private void OnEndpointSuccessfulPing(object sender, EndpointConnectionChangedEventArgs e)
        {
            lock (Agents)
            {
                 var savedAgent = FindAgentByName(e.EndpointInfo.Name);
                 if (savedAgent != null)
                 {
                     savedAgent.LastStatusUpdate = DateTime.UtcNow;
                 }
            }
        }

        void OnEndpointConnected(object sender, EndpointConnectionChangedEventArgs e)
        {
            lock (Agents)
            {
                var savedAgent = FindAgentByName(e.EndpointInfo.Name);
                if (savedAgent == null)
                {
                    var agentWithSameAddress =
                        Agents.FirstOrDefault(a => a.Endpoint.Address.Equals(e.EndpointInfo.Endpoint.Address));

                    if (agentWithSameAddress != null)
                    {
                        agentWithSameAddress.LastStatusUpdate = DateTime.UtcNow;
                        agentWithSameAddress.Name = e.EndpointInfo.Name;
                        agentWithSameAddress.State = AgentState.Connected;
                    }

                    Agents.Add(new AgentInformation
                                   {
                                       Endpoint = e.EndpointInfo.Endpoint,
                                       LastStatusUpdate = DateTime.UtcNow,
                                       State = AgentState.Connected,
                                       Name = e.EndpointInfo.Name
                                   });
                }
                else
                {
                    if (savedAgent.State == AgentState.Disconnected)
                    {
                        savedAgent.State = AgentState.Connected;
                    }
                    savedAgent.LastStatusUpdate = DateTime.UtcNow;
                    savedAgent.Endpoint = e.EndpointInfo.Endpoint;
                }
            }
            return;
        }

        private void OnEndpointDisconnected(object sender, EndpointConnectionChangedEventArgs e)
        {
            lock (Agents)
            {
                var savedAgent = FindAgentByName(e.EndpointInfo.Name);
                if (savedAgent != null)
                {
                    savedAgent.LastStatusUpdate = DateTime.UtcNow;
                    savedAgent.State = AgentState.Disconnected;
                }
            }
        }

        private AgentInformation FindAgentByName(string name)
        {
            return Agents.FirstOrDefault(agent => agent.Name.Equals(name));
        }

        /// <summary>
        /// Starts listening on agents' availability and monitors connected agents' statuses.
        /// </summary>
        public void Start()
        {
            connectionsTracker.Start();
        }

        /// <summary>
        /// Stops listening for agents
        /// </summary>
        public void Stop()
        {
            connectionsTracker.Stop();
        }
    }
}