using System;
using System.Runtime.Serialization;
using System.ServiceModel.Discovery;

namespace NDistribUnit.Common.DataContracts
{
    /// <summary>
    /// Holds information about an agent
    /// </summary>
    [DataContract]
    public class AgentInformation
    {
        /// <summary>
        /// The endpoint information, which can be used to connect to the agent
        /// </summary>
        public EndpointDiscoveryMetadata Endpoint { get; set; }

        /// <summary>
        /// The state of the agent
        /// </summary>
        [DataMember]
        public AgentState State { get; set; }

        /// <summary>
        /// Last date and time, when the agent was updated
        /// </summary>
        [DataMember]
        public DateTime LastStatusUpdate { get; set; }

        /// <summary>
        /// The friendly name of the agent
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        
        /// <summary>
        /// The friendly name of the agent
        /// </summary>
        [DataMember]
        public string Address
        {
            get { return Endpoint.Address.ToString(); }
            set { throw new NotSupportedException(); }
        }
    }
}