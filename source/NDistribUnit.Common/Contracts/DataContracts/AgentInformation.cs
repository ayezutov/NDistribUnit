using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Threading;
using NDistribUnit.Common.Extensions;
using NDistribUnit.Common.ServiceContracts;

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
					StateChanged.SafeInvoke(state, oldState);
    		}
    	}

		/// <summary>
		/// Occurs when the state is changed.
		/// </summary>
    	public event Action<AgentState, AgentState> StateChanged;

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

    	/// <summary>
		/// Gets the net TCP channel.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
    	public T GetNetTcpChannel<T>()
    	{
			return ChannelFactory<T>.CreateChannel(new NetTcpBinding("NDistribUnit.Default"), Endpoint.Address);
    	}

		
    }
}