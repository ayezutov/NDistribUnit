using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using NDistribUnit.Common.Agent;
using NDistribUnit.Common.DataContracts;

namespace NDistribUnit.Common.Server.AgentsTracking
{
    /// <summary>
    /// Stores the information about the agent
    /// </summary>
    public class AgentMetadata: IAgentStateSetter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentMetadata"/> class.
        /// </summary>
        public AgentMetadata(EndpointAddress address)
        {
            Address = address;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        public EndpointAddress Address { get; set; }


        /// <summary>
        /// Gets the remote part address.
        /// </summary>
        public EndpointAddress RemotePartAddress
        {
            get { return new EndpointAddress(new Uri(Address.Uri, AgentHost.RemoteParticleAddress)); }
        }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public Version Version { get; set; }

        /// <summary>
        /// Gets the status.
        /// </summary>
        public AgentState Status { get; private set; }

        /// <summary>
        /// Sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        AgentState IAgentStateSetter.Status
        {
            set { Status = value; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IAgentStateSetter
    {
        /// <summary>
        /// Sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        AgentState Status { set; }
    }
}