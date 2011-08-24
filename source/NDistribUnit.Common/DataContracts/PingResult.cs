using System;

namespace NDistribUnit.Common.DataContracts
{
    /// <summary>
    /// The result of a ping operation
    /// </summary>
    public class PingResult
    {
        /// <summary>
        /// Gets or sets the name of the endpoint.
        /// </summary>
        /// <value>
        /// The name of the endpoint.
        /// </value>
        public string EndpointName { get; set; }

        /// <summary>
        /// Gets or sets the state of the agent.
        /// </summary>
        /// <value>
        /// The state of the agent.
        /// </value>
        public AgentState AgentState { get; set; }

		/// <summary>
		/// Gets or sets the version.
		/// </summary>
		/// <value>
		/// The version.
		/// </value>
    	public Version Version { get; set; }
    }
}