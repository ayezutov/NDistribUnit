using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Threading;
using NDistribUnit.Common.Logging;
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
        private readonly ILog log;
        private readonly IList<EndpointInformation> endpoints = new List<EndpointInformation>();
        private DiscoveryClient discoveryClient;
        private FindCriteria findCriteria;
        private const int DiscoveryIntervalInMiliseconds = 5000;
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
        /// <param name="log"></param>
        public DiscoveryConnectionsTracker(string scope, ILog log)
        {
            this.scope = scope;
            this.log = log;
        }

        /// <summary>
        /// Starts the connections tracker
        /// </summary>
        public void Start()
        {
            findCriteria = new FindCriteria(typeof(ITestRunnerAgent))
                               {
                                   Scopes = {new Uri(scope)},
                                   Duration = TimeSpan.FromMilliseconds(DiscoveryIntervalInMiliseconds)
                               };
            discoveryClient = GetInitilizedDisoveryClient();
            log.Info("Starting clients tracking");

            var thread = new Thread(Discover);
            thread.Start();
            
        }

        private void Discover()
        {
            while (true)
            {
                try
                {
                    var client = new DiscoveryClient(new UdpDiscoveryEndpoint());
                    var result = client.Find(new FindCriteria(typeof (ITestRunnerAgent))
                               {
                                   Scopes = {new Uri(scope)},
                                   Duration = TimeSpan.FromMilliseconds(DiscoveryIntervalInMiliseconds)
                               });
                    client.Close();
                    foreach (var endpointDiscoveryMetadata in result.Endpoints)
                    {
                        AddEndpointForTracking(endpointDiscoveryMetadata);
                    }
                    log.Success("Discovery client was successfull");
                
                }catch (Exception)
                {
                    log.Warning("Discovery client got faulted");
                }
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
            if (!e.Cancelled)
            {
                if (e.Error != null)
                {
                    log.Warning("Discovery client got faulted");
                    discoveryClient.Close();
                    discoveryClient = GetInitilizedDisoveryClient();
                }
                else
                {
                    log.Info("Discovery client finished. Restarting...");
                }
                try
                {
                    var result = discoveryClient.Find(findCriteria);
                    foreach (var endpointDiscoveryMetadata in result.Endpoints)
                    {
                        AddEndpointForTracking(endpointDiscoveryMetadata);
                    }
                }
                catch(Exception)
                {
                    throw;
                }
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
                    log.Info(string.Format("New endpoint was detected: {0}", endpoint.Address));
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
            log.Info(string.Format("Endpoint was disconnected: {0}", endpointInformation.Endpoint.Address));
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