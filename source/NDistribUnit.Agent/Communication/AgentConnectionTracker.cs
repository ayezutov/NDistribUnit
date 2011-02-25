using System;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Server;

namespace NDistribUnit.Agent.Communication
{
    public class AgentConnectionTracker
    {
        private readonly ConnectionsTracker<ITestRunnerServer> connectionsTracker;
        private EndpointDiscoveryMetadata serverEndpoint;

        public AgentConnectionTracker(string scope, ServiceEndpoint endpoint)
        {
            connectionsTracker = new ConnectionsTracker<ITestRunnerServer>(scope, endpoint);
            connectionsTracker.EndpointConnected += OnEndpointConnected;
            connectionsTracker.EndpointDisconnected += OnEndpointDisconnected;
        }

        private void OnEndpointDisconnected(EndpointDiscoveryMetadata endpoint)
        {
            lock (this)
            {
                if (serverEndpoint != null && serverEndpoint == endpoint)
                    serverEndpoint = null;  
            }
        }

        private bool OnEndpointConnected(EndpointDiscoveryMetadata endpoint)
        {
            lock (this)
            {
                if (serverEndpoint != null)
                    return false;

                serverEndpoint = endpoint;
                return true;
            }
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