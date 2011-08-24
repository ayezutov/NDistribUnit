using System;

namespace NDistribUnit.Common.Communication.ConnectionTracking
{
    /// <summary>
    /// Event arguments class, which signals about a change in endpoint's connection state
    /// </summary>
    public class EndpointConnectionChangedEventArgs : System.EventArgs
    {
        /// <summary>
        /// The endpoint, which state was changed
        /// </summary>
        public EndpointInformation EndpointInfo { get; set; }

		/// <summary>
		/// Gets or sets the version.
		/// </summary>
		/// <value>
		/// The version.
		/// </value>
    	public Version Version { get; set; }
    }
}