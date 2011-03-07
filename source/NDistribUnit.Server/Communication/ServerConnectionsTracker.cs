using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Server.Communication
{
    public class ServerConnectionsTracker
    {
        private readonly DiscoveryConnectionsTracker<ITestRunnerAgent> connectionsTracker;
        public IList<AgentInformation> Agents { get; set; }

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

        public void Start()
        {
            connectionsTracker.Start();
        }

        public void Stop()
        {
            connectionsTracker.Stop();
        }
    }
}