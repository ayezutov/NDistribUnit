using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Threading;
using NDistribUnit.Common.Communication.ConnectionTracking;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;
using System.Linq;

namespace NDistribUnit.Common.Server.ConnectionTracking
{
    /// <summary>
    /// The base class for connection trackers
    /// </summary>
    /// <typeparam name="TIEndpoint">The type of the I endpoint.</typeparam>
    public abstract class ConnectionsTrackerBase<TIEndpoint>: IConnectionsTracker<TIEndpoint> where TIEndpoint : IPingable 
    {
        /// <summary>
        /// The log
        /// </summary>
        protected ILog log;

        /// <summary>
        /// The unique identifier of that instance
        /// </summary>
        protected readonly Guid guid = Guid.NewGuid();
        private readonly IList<EndpointInformation> endpoints = new List<EndpointInformation>();
        private readonly IConnectionsTrackerOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionsTrackerBase&lt;TIEndpoint&gt;"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="log">The log.</param>
        protected ConnectionsTrackerBase(IConnectionsTrackerOptions options, ILog log)
        {
            this.options = options;
            this.log = log;
        }

        private void OnEndpointPing(object state)
        {
            var endpointInformation = state as EndpointInformation;
            Debug.Assert(endpointInformation != null);
            lock (endpoints)
            {
                try
                {
                    log.Info(string.Format("{0}: Pinging {1} at {2}", guid, endpointInformation.Name,
                                           endpointInformation.Endpoint.Address));
                    endpointInformation.PingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    var result = endpointInformation.Pingable.Ping(TimeSpan.FromMilliseconds(options.PingIntervalInMiliseconds));
                    if (result.EndpointName.Equals(endpointInformation.Name))
                    {
                        endpointInformation.PingTimer.Change(options.PingIntervalInMiliseconds, Timeout.Infinite);
                        endpointInformation.LastStatusUpdateTime = DateTime.UtcNow;
                        if (EndpointSuccessfulPing != null)
                            EndpointSuccessfulPing(this, new EndpointConnectionChangedEventArgs
                                                             {
                                                                 EndpointInfo = endpointInformation
                                                             });
                    }
                    else
                    {
                        RemoveEndpointFromTracking(endpointInformation);
                        AddEndpointForTracking(endpointInformation.Endpoint, result.EndpointName);
                    }
                }
                catch (CommunicationException)
                {
                    RemoveEndpointFromTracking(endpointInformation);
                }
            }
        }

        /// <summary>
        /// Adds the endpoint for tracking.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="agentName">Name of the agent.</param>
        protected void AddEndpointForTracking(EndpointDiscoveryMetadata endpoint, string agentName = null)
        {
            var endpointInformation = new EndpointInformation
                                          {
                                              Endpoint = endpoint,
                                              LastStatusUpdateTime = DateTime.UtcNow,
                                              Name = agentName ?? AgentAdditionalDataManager.GetAgentName(endpoint.Extensions)
                                          };
            lock (endpoints)
            {
                log.Info(string.Format("{1}: Got: {0}", endpoint.Address, guid));
                var savedEndpoint = endpoints.FirstOrDefault(e => e.Equals(endpointInformation));
                if (savedEndpoint == null || !savedEndpoint.Endpoint.Address.Equals(endpoint.Address))
                {
                    if (savedEndpoint != null)
                        RemoveEndpointFromTracking(savedEndpoint);

                    log.Info(string.Format("{1}: New endpoint was detected: {0}", endpoint.Address, guid));
                    endpoints.Add(endpointInformation);

                    endpointInformation.Pingable = ChannelFactory<TIEndpoint>.CreateChannel(new NetTcpBinding(),
                                                                                            endpoint.Address);
                    endpointInformation.PingTimer = new Timer(OnEndpointPing, endpointInformation,
                                                              0,
                                                              Timeout.Infinite);

                    if (EndpointConnected != null)
                        EndpointConnected(this, new EndpointConnectionChangedEventArgs { EndpointInfo = endpointInformation });
                }
            }
        }

        private void RemoveEndpointFromTracking(EndpointInformation endpointInformation)
        {
            log.Info(string.Format("{1}: Endpoint was disconnected: {0}", endpointInformation.Endpoint.Address, guid));
            lock (endpoints)
            {
                if (endpointInformation.PingTimer != null)
                {
                    endpointInformation.PingTimer.Dispose();
                }
                endpointInformation.LastStatusUpdateTime = DateTime.UtcNow;
                endpoints.Remove(endpointInformation);
            }

            if (EndpointDisconnected != null)
                EndpointDisconnected(this, new EndpointConnectionChangedEventArgs { EndpointInfo = endpointInformation});
        }

        /// <summary>
        /// Event, which is fired, whenever a new endpoint is connected
        /// </summary>
        public event EventHandler<EndpointConnectionChangedEventArgs> EndpointConnected;

        /// <summary>
        /// Event, which is fired, whenever an endpoint is successfully pinged
        /// </summary>
        public event EventHandler<EndpointConnectionChangedEventArgs> EndpointSuccessfulPing;

        /// <summary>
        /// An event, which is fired, whenever an endpoint is disconnected
        /// </summary>
        public event EventHandler<EndpointConnectionChangedEventArgs> EndpointDisconnected;

        /// <summary>
        /// Starts the connections tracker
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Stops the connections tracker
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Stops endpoints' pinging.
        /// </summary>
        protected void StopPingingEndpoints()
        {
            foreach (var endpointInformation in endpoints)
            {
                if (endpointInformation.PingTimer != null)
                {
                    endpointInformation.PingTimer.Dispose();
                    endpointInformation.PingTimer = null;
                }
            }
        }
    }
}