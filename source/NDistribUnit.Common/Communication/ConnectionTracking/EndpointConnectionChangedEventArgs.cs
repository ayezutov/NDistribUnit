using System;

namespace NDistribUnit.Common.Communication.ConnectionTracking
{
    /// <summary>
    /// Event arguments class, which signals about a change in endpoint's connection state
    /// </summary>
    public class EndpointConnectionChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The endpoint, which state was changed
        /// </summary>
        public EndpointInformation EndpointInfo { get; set; }
    }
}