using System;
using System.ServiceModel.Discovery;

namespace NDistribUnit.Server.Communication
{
    public class AgentInformation
    {
        public EndpointDiscoveryMetadata Endpoint { get; set; }
        public AgentState State { get; set; }
        public DateTime LastStatusUpdate { get; set; }
    }
}