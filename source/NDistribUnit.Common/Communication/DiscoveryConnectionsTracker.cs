using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Threading;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Common.Communication
{
    /// <summary>
    /// A connection tracker, which relies on discovery mechanism
    /// </summary>
    /// <typeparam name="TIEndpoint"></typeparam>
    public class DiscoveryConnectionsTracker<TIEndpoint>
        where TIEndpoint : IPingable
    {
        private readonly string scope;
        private readonly IList<EndpointInformation> endpoints = new List<EndpointInformation>();
        private DiscoveryClient discoveryClient;
        private FindCriteria findCriteria;
        private const int DiscoveryIntervalInMiliseconds = 20000;
        private const int PingIntervalInMiliseconds = 5000;

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
        /// <param name="scope"></param>
        public DiscoveryConnectionsTracker(string scope)
        {
            this.scope = scope;
        }

        /// <summary>
        /// Starts the connections tracker
        /// </summary>
        public void Start()
        {
            discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());
            findCriteria = new FindCriteria(typeof(ITestRunnerAgent))
                               {
                                   Scopes = {new Uri(scope)},
                                   Duration = TimeSpan.FromMilliseconds(DiscoveryIntervalInMiliseconds)
                               };
            discoveryClient.FindProgressChanged += OnFindProgressChanged;
            discoveryClient.FindCompleted += OnFindCompleted;
            discoveryClient.FindAsync(findCriteria);
        }

        private void OnFindCompleted(object sender, FindCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
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
            discoveryClient.Close();
        }

        private void OnEndpointPing(object state)
        {
            var endpointInformation = state as EndpointInformation;
            Debug.Assert(endpointInformation != null);

            try
            {
                endpointInformation.Pingable.Ping();
                endpointInformation.PingTimer.Change(PingIntervalInMiliseconds, Timeout.Infinite);
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
                    endpoints.Add(endpointInformation);

                    endpointInformation.Pingable = ChannelFactory<TIEndpoint>.CreateChannel(new NetTcpBinding(),
                                                                                            endpoint.Address);
                    endpointInformation.PingTimer = new Timer(OnEndpointPing, endpointInformation,
                                                              PingIntervalInMiliseconds,
                                                              Timeout.Infinite);

                    if (EndpointConnected != null)
                        EndpointConnected(this, new EndpointConnectionChangedEventArgs { EndpointInfo = endpointInformation });
                }
            }
        }

        private void RemoveEndpointFromTracking(EndpointInformation endpointInformation)
        {
            lock (endpointInformation)
            {
                endpointInformation.PingTimer.Dispose();
                endpointInformation.LastStatusUpdateTime = DateTime.UtcNow;
                endpoints.Remove(endpointInformation);
            }

            if (EndpointDisconnected != null)
                EndpointDisconnected(this, new EndpointConnectionChangedEventArgs { EndpointInfo = endpointInformation});
        }
    }
}