using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Threading;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Common.Communication.ConnectionTracking.Discovery
{
    /// <summary>
    /// A connection tracker, which relies on discovery mechanism
    /// </summary>
    /// <typeparam name="TIEndpoint"></typeparam>
    public class DiscoveryConnectionsTracker<TIEndpoint> : IConnectionsTracker<TIEndpoint> where TIEndpoint : IPingable
    {
        private readonly DiscoveryOptions options;
        private readonly ILog log;
        private readonly Guid guid = Guid.NewGuid();

        private readonly IList<EndpointInformation> endpoints = new List<EndpointInformation>();
        private DiscoveryClient discoveryClient;
        private FindCriteria findCriteria;
        private bool stopped;

        /// <summary>
        /// Event, which is fired, whenever a new endpoint is connected
        /// </summary>
        public event EventHandler<EndpointConnectionChangedEventArgs> EndpointConnected;

        /// <summary>
        /// Event, which is firedm whenever an endpoint is successfully pinged
        /// </summary>
        public event EventHandler<EndpointConnectionChangedEventArgs> EndpointSuccessfulPing;

        /// <summary>
        /// An event, which is fired, whenever an endpoint is disconnected
        /// </summary>
        public event EventHandler<EndpointConnectionChangedEventArgs> EndpointDisconnected;

        /// <summary>
        /// Initializes a new instance of connections tracker
        /// </summary>
        /// <param name="options">The options for use while discovery</param>
        /// <param name="log"></param>
        public DiscoveryConnectionsTracker(DiscoveryOptions options, ILog log)
        {
            this.options = options;
            this.log = log;
        }

        /// <summary>
        /// Starts the connections tracker
        /// </summary>
        public void Start()
        {
            findCriteria = new FindCriteria(typeof(TIEndpoint))
                               {
                                   Scopes = {options.Scope},
                                   Duration = TimeSpan.FromMilliseconds(options.DiscoveryIntervalInMiliseconds)
                               };
            discoveryClient = GetInitilizedDisoveryClient();
            log.Info(string.Format("{0}: Starting clients tracking", guid));

            Discover();
            
        }

        private void Discover()
        {
            
                try
                {
                    discoveryClient.FindAsync(findCriteria);
                }
                catch (Exception)
                {
                    log.Warning(string.Format("{0}: Discovery client got faulted", guid));
                }
        }

        private DiscoveryClient GetInitilizedDisoveryClient()
        {
            var client = new DiscoveryClient(new UdpDiscoveryEndpoint());
            client.FindProgressChanged += OnFindProgressChanged;
            client.FindCompleted += OnFindCompleted;
            return  client;
        }

        private void OnFindCompleted(object sender, FindCompletedEventArgs e)
        {
            if (!e.Cancelled && !stopped)
            {
                if (e.Error != null)
                {
                    log.Warning(string.Format("{0}: Discovery client got faulted", guid));
                    discoveryClient.Close();
                    discoveryClient = GetInitilizedDisoveryClient();
                }
                else
                {
                    log.Info(string.Format("{0}: Discovery client finished. Restarting...", guid));
                }
                discoveryClient.FindAsync(findCriteria);
            }
        }



        private void OnFindProgressChanged(object sender, FindProgressChangedEventArgs e)
        {
            var endpoint = e.EndpointDiscoveryMetadata;
            AddEndpointForTracking(endpoint);
        }

        /// <summary>
        /// Stops the connections tracker
        /// </summary>
        public void Stop()
        {
            stopped = true;
            discoveryClient.Close();

            foreach (var endpointInformation in endpoints)
            {
                if (endpointInformation.PingTimer != null)
                {
                    endpointInformation.PingTimer.Dispose();
                    endpointInformation.PingTimer = null;
                }
            }
        }

        private void OnEndpointPing(object state)
        {
            var endpointInformation = state as EndpointInformation;
            Debug.Assert(endpointInformation != null);

            try
            {
                endpointInformation.Pingable.Ping();
                endpointInformation.PingTimer.Change(options.PingIntervalInMiliseconds, Timeout.Infinite);
                endpointInformation.LastStatusUpdateTime = DateTime.UtcNow;
                if (EndpointSuccessfulPing != null)
                    EndpointSuccessfulPing(this, new EndpointConnectionChangedEventArgs
                                                     {
                                                         EndpointInfo = endpointInformation
                                                     });
            }
            catch (CommunicationException)
            {
                RemoveEndpointFromTracking(endpointInformation);
            }
        }

        private void AddEndpointForTracking(EndpointDiscoveryMetadata endpoint)
        {
            var endpointInformation = new EndpointInformation
                                              {
                                                  Endpoint = endpoint,
                                                  LastStatusUpdateTime = DateTime.UtcNow
                                              };
            lock (endpoints)
            {
                if (!endpoints.Contains(endpointInformation))
                {
                    log.Info(string.Format("{1}: New endpoint was detected: {0}", endpoint.Address, guid));
                    endpoints.Add(endpointInformation);

                    endpointInformation.Pingable = ChannelFactory<TIEndpoint>.CreateChannel(new NetTcpBinding(),
                                                                                            endpoint.Address);
                    endpointInformation.PingTimer = new Timer(OnEndpointPing, endpointInformation,
                                                              options.PingIntervalInMiliseconds,
                                                              Timeout.Infinite);

                    if (EndpointConnected != null)
                        EndpointConnected(this, new EndpointConnectionChangedEventArgs { EndpointInfo = endpointInformation });
                }
            }
        }

        private void RemoveEndpointFromTracking(EndpointInformation endpointInformation)
        {
            log.Info(string.Format("{1}: Endpoint was disconnected: {0}", endpointInformation.Endpoint.Address, guid));
            lock (endpointInformation)
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
    }
}