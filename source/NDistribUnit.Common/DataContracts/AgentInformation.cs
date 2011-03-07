using System;
using System.Runtime.Serialization;
using System.ServiceModel.Discovery;
using NDistribUnit.Server.Communication;

namespace NDistribUnit.Common.DataContracts
{
    [DataContract]
    public class AgentInformation
    {
        public EndpointDiscoveryMetadata Endpoint { get; set; }

        [DataMember]
        public AgentState State { get; set; }

        [DataMember]
        public DateTime LastStatusUpdate { get; set; }

        [DataMember]
        public string Name
        {
            get { return Endpoint.Address.ToString(); }
            set { throw new NotSupportedException(); }
        }
    }
}