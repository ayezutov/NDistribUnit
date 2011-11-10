using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Threading;
using NDistribUnit.Common.Agent;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Communication.ConnectionTracking;
using NDistribUnit.Common.Extensions;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Common.Server.ConnectionTracking
{
    /// <summary>
    /// The base class for connection trackers
    /// </summary>
    /// <typeparam name="TIEndpoint">The type of the endpoint.</typeparam>
    public abstract class NetworkExplorerBase<TIEndpoint> : INetworkExplorer<TIEndpoint> where TIEndpoint : class, IPingable
    {
        /// <summary>
        /// The log
        /// </summary>
        protected ILog log;

        private readonly IConnectionProvider connectionProvider;

        /// <summary>
        /// The unique identifier of that instance
        /// </summary>
        protected readonly Guid guid = Guid.NewGuid();

        private readonly IList<EndpointInformation> endpoints = new List<EndpointInformation>();

        ///
        protected readonly IConnectionsHostOptions ConnectionsHostOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkExplorerBase{TIEndpoint}"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="log">The log.</param>
        /// <param name="connectionProvider">The connection provider.</param>
        protected NetworkExplorerBase(IConnectionsHostOptions options, ILog log, IConnectionProvider connectionProvider)
        {
            ConnectionsHostOptions = options;
            this.log = log;
            this.connectionProvider = connectionProvider;
        }

        private void OnEndpointPing(object state)
        {
            var endpointInformation = state as EndpointInformation;
            Debug.Assert(endpointInformation != null);
            lock (endpoints)
            {
                try
                {
                    log.Debug(string.Format("{0}: Pinging {1} at {2}", guid, endpointInformation.Name,
                                            endpointInformation.Endpoint.Address));
                    try
                    {
                        endpointInformation.PingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    }
                    catch (ObjectDisposedException)
                    {
                        return;
                    }
                    var result =
                        endpointInformation.Pingable.Ping(
                            TimeSpan.FromMilliseconds(ConnectionsHostOptions.PingIntervalInMiliseconds));
                    if (result.EndpointName.Equals(endpointInformation.Name))
                    {
                        endpointInformation.PingTimer.Change(ConnectionsHostOptions.PingIntervalInMiliseconds,
                                                             Timeout.Infinite);
                        endpointInformation.LastStatusUpdateTime = DateTime.UtcNow;
                        if (EndpointSuccessfulPing != null)
                            EndpointSuccessfulPing(this, new EndpointConnectionChangedEventArgs
                                                             {
                                                                 EndpointInfo = endpointInformation,
                                                                 Version = result.Version
                                                             });
                    }
                    else
                    {
                        RemoveEndpointFromTracking(endpointInformation);
                        AddEndpointForTracking(endpointInformation.Endpoint, result.EndpointName);
                    }
                }
                catch (CommunicationException ex)
                {
                    log.Error("Error while pinging endpoint", ex);
                    RemoveEndpointFromTracking(endpointInformation);
                }
            }
        }

        /// <summary>
        /// Adds the endpoint for tracking.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="agentName">Name of the agent.</param>
        /// <param name="version">The version.</param>
        protected void AddEndpointForTracking(EndpointDiscoveryMetadata endpoint, string agentName = null,
                                              Version version = null)
        {
            var endpointInformation = new EndpointInformation
                                          {
                                              Endpoint = endpoint,
                                              LastStatusUpdateTime = DateTime.UtcNow,
                                              Name = agentName ?? AdditionalDataManager.GetName(endpoint.Extensions),
                                              Version = version ?? AdditionalDataManager.GetVersion(endpoint.Extensions)
                                          };
            lock (endpoints)
            {
                log.Info(string.Format("{1}: Got: {0}", endpoint.Address, guid));
                var endpointWithSameName = endpoints.FirstOrDefault(e => e.Equals(endpointInformation));
                var endpointWithSameAddress = endpoints.FirstOrDefault(e =>
                                                                       e.Endpoint.Address.Equals(
                                                                           endpointInformation.Endpoint.Address) &&
                                                                       !e.Endpoint.Equals(endpointInformation));

                if (endpointWithSameAddress != null)
                {
                    RemoveEndpointFromTracking(endpointWithSameAddress);
                }

                if (endpointWithSameName == null || !endpointWithSameName.Endpoint.Address.Equals(endpoint.Address))
                {
                    if (endpointWithSameName != null)
                        RemoveEndpointFromTracking(endpointWithSameName);

                    log.Info(string.Format("{1}: New endpoint was detected: {0}", endpoint.Address, guid));
                    endpoints.Add(endpointInformation);

                    try
                    {
                        endpointInformation.Pingable = connectionProvider.GetConnection<TIEndpoint>(new EndpointAddress(new Uri(endpoint.Address.Uri, AgentHost.RemoteParticleAddress)));
                        endpointInformation.PingTimer = new Timer(OnEndpointPing, endpointInformation,
                                                                  0, Timeout.Infinite);
                    }
                    catch(Exception ex)
                    {
                        log.Error("Error while creating pingable", ex);
                    }
                    EndpointConnected.SafeInvoke(this, new EndpointConnectionChangedEventArgs
                                                           {
                                                               EndpointInfo = endpointInformation,
                                                               Version = endpointInformation.Version
                                                           });
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

            EndpointDisconnected.SafeInvoke(this, new EndpointConnectionChangedEventArgs
            {
                EndpointInfo = endpointInformation,
                Version = endpointInformation.Version
            });
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