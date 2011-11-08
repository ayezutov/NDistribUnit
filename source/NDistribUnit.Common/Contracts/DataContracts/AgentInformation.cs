using System;
using System.Runtime.Serialization;
using System.ServiceModel.Discovery;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Extensions;

namespace NDistribUnit.Common.Contracts.DataContracts
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

    	private AgentState state;

    	/// <summary>
        /// The state of the agent
        /// </summary>
        [DataMember]
        public AgentState State
    	{
    		get { return state; }
    		set
    		{
    			var oldState = state;
    			state = value;
				if (oldState != state)
					StateChanged.SafeInvoke(this, oldState);
    		}
    	}

		/// <summary>
		/// Occurs when the state is changed.
		/// </summary>
    	public event EventHandler<EventArgs<AgentState>> StateChanged;

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

		/// <summary>
		/// Gets or sets the version.
		/// </summary>
		/// <value>
		/// The version.
		/// </value>
    	public Version Version { get; set; }
    }
}