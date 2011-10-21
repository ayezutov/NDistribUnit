using System.ServiceModel;
using System.ServiceModel.Discovery;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Common.Server.ConnectionTracking.Announcement
{
    /// <summary>
    /// The connections tracker, which is based on announcement mechanism
    /// </summary>
    public class AnnouncementConnectionTracker<TIEndpoint>: ConnectionsTrackerBase<TIEndpoint> where TIEndpoint : IPingable
    {
        private AnnouncementService announcementService;
        private ServiceHost announcementServiceHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnouncementConnectionTracker{TIEndpoint}"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="log">The log.</param>
        public AnnouncementConnectionTracker(IConnectionsHostOptions options, ILog log) : base(options, log)
        {
        }

        /// <summary>
        /// Starts the connections tracker
        /// </summary>
        public override void Start()
        {
            announcementService = new AnnouncementService();
            announcementService.OnlineAnnouncementReceived += OnClientNotificationReceived;

            announcementServiceHost = new ServiceHost(announcementService);
            announcementServiceHost.AddServiceEndpoint(new UdpAnnouncementEndpoint());
            announcementServiceHost.Open();
        }

        private void OnClientNotificationReceived(object sender, AnnouncementEventArgs e)
        {
            if (e.EndpointDiscoveryMetadata.Scopes.Contains(ConnectionsHostOptions.Scope))
                AddEndpointForTracking(e.EndpointDiscoveryMetadata);
        }

        /// <summary>
        /// Stops the connections tracker
        /// </summary>
        public override void Stop()
        {
            announcementServiceHost.Close();
            StopPingingEndpoints();
        }
    }
}