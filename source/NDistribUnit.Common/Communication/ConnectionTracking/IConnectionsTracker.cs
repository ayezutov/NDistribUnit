using System;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Common.Communication.ConnectionTracking
{
    /// <summary>
    /// Represents a connection tracker
    /// </summary>
    /// <typeparam name="TIEndpoint"></typeparam>
    public interface IConnectionsTracker<TIEndpoint> where TIEndpoint : IPingable
    {
        /// <summary>
        /// Event, which is fired, whenever a new endpoint is connected
        /// </summary>
        event EventHandler<EndpointConnectionChangedEventArgs> EndpointConnected;

        /// <summary>
        /// Event, which is firedm whenever an endpoint is successfully pinged
        /// </summary>
        event EventHandler<EndpointConnectionChangedEventArgs> EndpointSuccessfulPing;

        /// <summary>
        /// An event, which is fired, whenever an endpoint is disconnected
        /// </summary>
        event EventHandler<EndpointConnectionChangedEventArgs> EndpointDisconnected;

        /// <summary>
        /// Starts the connections tracker
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the connections tracker
        /// </summary>
        void Stop();
    }
}