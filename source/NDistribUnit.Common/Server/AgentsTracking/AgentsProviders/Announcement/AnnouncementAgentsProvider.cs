using System;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Extensions;

namespace NDistribUnit.Common.Server.AgentsTracking.AgentsProviders.Announcement
{
    /// <summary>
    /// A provider, which uses WCF announcements. It listens for broadcast request issued by agents
    /// identifying, that they are available.
    /// </summary>
    public class AnnouncementAgentsProvider : IAgentsProvider
    {
        private readonly IConnectionsHostOptions options;
        private readonly ILog log;
        private AnnouncementService announcementService;
        private ServiceHost announcementServiceHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnouncementAgentsProvider"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="log">The log.</param>
        public AnnouncementAgentsProvider(IConnectionsHostOptions options, ILog log)
        {
            this.options = options;
            this.log = log;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            announcementService = new AnnouncementService();
            announcementService.OnlineAnnouncementReceived += OnClientNotificationReceived;

            announcementServiceHost = new ServiceHost(announcementService);
            announcementServiceHost.AddServiceEndpoint(new UdpAnnouncementEndpoint());
            announcementServiceHost.Open();
        }

        private void OnClientNotificationReceived(object sender, AnnouncementEventArgs e)
        {
            if (e.EndpointDiscoveryMetadata.Scopes.Contains(options.Scope))
            {
                log.Debug(string.Format("Announcement was received from {0}", e.EndpointDiscoveryMetadata.Address));
                Connected.SafeInvoke(this, e.EndpointDiscoveryMetadata.Address);
            }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            announcementServiceHost.Close();
        }

        /// <summary>
        /// Occurs when an agent is connected.
        /// </summary>
        public event EventHandler<EventArgs<EndpointAddress>> Connected;
    }
}