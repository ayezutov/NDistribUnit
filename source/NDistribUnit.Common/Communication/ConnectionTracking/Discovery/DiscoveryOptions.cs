using System;

namespace NDistribUnit.Common.Communication.ConnectionTracking.Discovery
{
    /// <summary>
    /// The options for <see cref="DiscoveryConnectionsTracker{TIEndpoint}"/>
    /// </summary>
    public class DiscoveryOptions
    {
        /// <summary>
        /// Gets or sets the discovery interval in miliseconds.
        /// </summary>
        /// <value>
        /// The discovery interval in miliseconds.
        /// </value>
        public double DiscoveryIntervalInMiliseconds { get; set; }

        /// <summary>
        /// Gets or sets the scope.
        /// </summary>
        /// <value>
        /// The scope.
        /// </value>
        public Uri Scope { get; set; }

        /// <summary>
        /// Gets the ping interval in miliseconds.
        /// </summary>
        public int PingIntervalInMiliseconds { get; set; }
    }
}