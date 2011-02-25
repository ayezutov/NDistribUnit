using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Threading;
using System.Xml;
using System.Linq;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ConnectionsTracker<TContract>: IConnectionTracker
    {
        private readonly string scope;
        private readonly ServiceEndpoint endPointToReturn;
        private readonly IList<EndpointInformation> endpoints = new List<EndpointInformation>();
        private const int TimerIntervalInMiliseconds = 2000;


        public ConnectionsTracker(string scope, ServiceEndpoint endPointToReturn)
        {
            this.scope = scope;
            this.endPointToReturn = endPointToReturn;
        }

        protected ServiceEndpoint ConnectionTrackerEndpoint { get; set; }
        private ServiceHost AnnouncementServiceHost { get; set; }
        private ServiceHost ConnectionTrackerHost { get; set; }

        private AnnouncementService AnnouncementService { get; set; }

        public event Func<EndpointDiscoveryMetadata, bool> EndpointConnected;
        public event Action<EndpointDiscoveryMetadata> EndpointDisconnected;

        public void Start()
        {
            // start listening for new endpoints
            AnnouncementService = new AnnouncementService();
            AnnouncementService.OnlineAnnouncementReceived += OnAgentConnecting;
            AnnouncementService.OfflineAnnouncementReceived += OnAgentDisconnecting;

            AnnouncementServiceHost = new ServiceHost(AnnouncementService);
            AnnouncementServiceHost.AddServiceEndpoint(new UdpAnnouncementEndpoint());
            AnnouncementServiceHost.Open();

            // start answering clients on ping
            ConnectionTrackerHost = new ServiceHost(this, new Uri(string.Format("net.tcp://{0}:{1}", Environment.MachineName, WcfUtilities.FindPort())));
            ConnectionTrackerEndpoint = ConnectionTrackerHost.AddServiceEndpoint(typeof(IConnectionTracker), new NetTcpBinding(), "");
            ConnectionTrackerHost.Open();

            // say i'm online
            var discoveryMetadata = GetDiscoveryMetadata();
            var announcementClient = new AnnouncementClient(new UdpAnnouncementEndpoint());
            announcementClient.AnnounceOnline(discoveryMetadata);
            announcementClient.Close();
        }

        public void Stop()
        {
            // stop to listen for new endpoints
            AnnouncementServiceHost.Close();

            // say i'm offline
            EndpointDiscoveryMetadata discoveryMetadata = GetDiscoveryMetadata();
            var announcementClient = new AnnouncementClient(new UdpAnnouncementEndpoint());
            announcementClient.AnnounceOffline(discoveryMetadata);
            announcementClient.Close();
            
            // stop answering pings
            ConnectionTrackerHost.Close();
        }

        private EndpointDiscoveryMetadata GetDiscoveryMetadata()
        {
            var discoveryMetadata = EndpointDiscoveryMetadata.FromServiceEndpoint(ConnectionTrackerEndpoint);
            Debug.Assert(discoveryMetadata != null);
            discoveryMetadata.ContractTypeNames.Add(GetQualifiedName(endPointToReturn.Contract.ContractType));
            discoveryMetadata.Scopes.Add(new Uri(scope));
            return discoveryMetadata;
        }

        private static XmlQualifiedName GetQualifiedName(Type contractType)
        {
            return new XmlQualifiedName(contractType.Name, contractType.Namespace);
        }


        private void OnAgentDisconnecting(object sender, AnnouncementEventArgs e)
        {
            var connectionTrackerEndpoint = e.EndpointDiscoveryMetadata;
            if (connectionTrackerEndpoint.Scopes.Contains(new Uri(scope)) &&
                connectionTrackerEndpoint.ContractTypeNames.Contains(GetQualifiedName(typeof(TContract))))
            {
                var endpointInfo = FindEndpointSavedInfo(connectionTrackerEndpoint.Address);

                if (endpointInfo != null)
                {
                    RemoveEndpointFromInternalCollection(endpointInfo);
                    if (EndpointDisconnected != null)
                    {
                        EndpointDisconnected(endpointInfo.Endpoint);
                    }
                }
            }
        }

        private void RemoveEndpointFromInternalCollection(EndpointInformation endpointInfo)
        {
            lock (endpointInfo)
            {
                var foundEndpoint = FindEndpointSavedInfo(endpointInfo.ConnectionTrackerEndpoint.Address);
                if (foundEndpoint != null)
                {
                    foundEndpoint.PingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    foundEndpoint.PingTimer.Dispose();

                    endpoints.Remove(foundEndpoint);

                    var connectionTrackerChannel = foundEndpoint.ConnectionTracker as ICommunicationObject;
                    Debug.Assert(connectionTrackerChannel != null);
                    if (connectionTrackerChannel.State == CommunicationState.Opened ||
                        connectionTrackerChannel.State == CommunicationState.Opening)
                    {
                        connectionTrackerChannel.Close();
                    }
                }
            }
        }

        private EndpointInformation FindEndpointSavedInfo(EndpointAddress endpointAddress)
        {
            return endpoints.FirstOrDefault(e => e.ConnectionTrackerEndpoint.Address.Equals(endpointAddress));
        }

        private void OnAgentConnecting(object sender, AnnouncementEventArgs e)
        {
            var endpointDiscoveryMetadata = e.EndpointDiscoveryMetadata;
            if (endpointDiscoveryMetadata.Scopes.Contains(new Uri(scope)) && 
                endpointDiscoveryMetadata.ContractTypeNames.Contains(GetQualifiedName(typeof(TContract))))
            {
                var connectionTracker = ChannelFactory<IConnectionTracker>.CreateChannel(new NetTcpBinding(),
                                                                                         e.EndpointDiscoveryMetadata.Address);
                var realEndpoint = connectionTracker.GetEndpoint();
                if (EndpointConnected != null)
                {
                    if (EndpointConnected(realEndpoint))
                        AddEndpointForTracking(new EndpointInformation
                        {
                            ConnectionTracker = connectionTracker, 
                            Endpoint = realEndpoint, 
                            ConnectionTrackerEndpoint = endpointDiscoveryMetadata
                        });
                }
            }
        }

        private void AddEndpointForTracking(EndpointInformation endpointInfo)
        {
            lock (endpoints)
            {
                if (FindEndpointSavedInfo(endpointInfo.ConnectionTrackerEndpoint.Address) == null)
                {
                    endpoints.Add(endpointInfo);
                    endpointInfo.LastStatusUpdateTime = DateTime.UtcNow;
                    endpointInfo.PingTimer = new Timer(OnPingTimerCallback, endpointInfo,
                                                       TimeSpan.FromMilliseconds(TimerIntervalInMiliseconds),
                                                       TimeSpan.FromMilliseconds(TimerIntervalInMiliseconds));
                }
            }
        }

        private void OnPingTimerCallback(object state)
        {
            var endpointInfo = (EndpointInformation) state;
            try
            {
                endpointInfo.ConnectionTracker.Ping();
            }
            catch (CommunicationException)
            {
                RemoveEndpointFromInternalCollection(endpointInfo);
            }
            endpointInfo.LastStatusUpdateTime = DateTime.UtcNow;
        }

        bool IConnectionTracker.Ping()
        {
            return true;
        }

        EndpointDiscoveryMetadata IConnectionTracker.GetEndpoint()
        {
            return EndpointDiscoveryMetadata.FromServiceEndpoint(endPointToReturn);
        }
    }
}