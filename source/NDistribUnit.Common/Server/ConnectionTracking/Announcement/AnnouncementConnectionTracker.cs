using System.ServiceModel;
using System.ServiceModel.Discovery;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Common.Server.ConnectionTracking.Announcement
{
    /// <summary>
    /// The connections tracker, which is based on announcement mechanism
    /// </summary>
    public class AnnouncementConnectionTracker<TIEndpoint>: NetworkExplorerBase<TIEndpoint> where TIEndpoint : class, IPingable
    {
        private AnnouncementService announcementService;
        private ServiceHost announcementServiceHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnouncementConnectionTracker{TIEndpoint}"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="log">The log.</param>
        /// <param name="connectionProvider">The connection provider.</param>
        public AnnouncementConnectionTracker(IConnectionsHostOptions options, ILog log, IConnectionProvider connectionProvider) : base(options, log, connectionProvider)
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