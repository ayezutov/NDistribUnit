using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Server.Communication
{
    /// <summary>
    /// Tracks connections on NDistribUnit server. It always has a list of agents with actual statuses.
    /// </summary>
    public class ServerConnectionsTracker
    {
        private readonly DiscoveryConnectionsTracker<ITestRunnerAgent> connectionsTracker;

        /// <summary>
        /// Gets the list of agents
        /// </summary>
        public IList<AgentInformation> Agents { get; private set; }

        /// <summary>
        /// Initializes a new instance of a connection tracker
        /// </summary>
        /// <param name="scope"></param>
        public ServerConnectionsTracker(string scope)
        {
            Agents = new List<AgentInformation>();
            connectionsTracker = new DiscoveryConnectionsTracker<ITestRunnerAgent>(scope);
            connectionsTracker.EndpointConnected += OnEndpointConnected;
            connectionsTracker.EndpointDisconnected += OnEndpointDisconnected;
            connectionsTracker.EndpointSuccessfulPing += OnEndpointSuccessfulPing;
        }

        private void OnEndpointSuccessfulPing(object sender, EndpointConnectionChangedEventArgs e)
        {
            lock (Agents)
            {
                 var savedAgent = FindAgentByAddress(e.EndpointInfo.Endpoint.Address);
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
                var savedAgent = FindAgentByAddress(e.EndpointInfo.Endpoint.Address);
                if (savedAgent == null)
                {
                    Agents.Add(new AgentInformation
                                   {
                                       Endpoint = e.EndpointInfo.Endpoint,
                                       LastStatusUpdate = DateTime.UtcNow,
                                       State = AgentState.Connected
                                   });
                }
                else
                {
                    if (savedAgent.State == AgentState.Disconnected)
                    {
                        savedAgent.State = AgentState.Connected;
                    }
                    savedAgent.LastStatusUpdate = DateTime.UtcNow;
                }
            }
            return;
        }

        private void OnEndpointDisconnected(object sender, EndpointConnectionChangedEventArgs e)
        {
            lock (Agents)
            {
                var savedAgent = FindAgentByAddress(e.EndpointInfo.Endpoint.Address);
                if (savedAgent != null)
                {
                    savedAgent.LastStatusUpdate = DateTime.UtcNow;
                    savedAgent.State = AgentState.Disconnected;
                }
            }
        }

        private AgentInformation FindAgentByAddress(EndpointAddress address)
        {
            return Agents.FirstOrDefault(agent => agent.Endpoint.Address.ToString().Equals(address.ToString()));
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