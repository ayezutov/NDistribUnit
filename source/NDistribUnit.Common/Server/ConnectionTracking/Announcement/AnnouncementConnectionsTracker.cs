using System;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Server.ConnectionTracking;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Common.Communication.ConnectionTracking.Announcement
{
    /// <summary>
    /// The connections tracker, which is based on announcement mechanism
    /// </summary>
    public class AnnouncementConnectionsTracker<TIEndpoint>: ConnectionsTrackerBase<TIEndpoint> where TIEndpoint : IPingable
    {
        private AnnouncementConnectionsTrackerOptions options;
        private AnnouncementService announcementService;
        private ServiceHost announcementServiceHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnouncementConnectionsTracker&lt;TIEndpoint&gt;"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="log">The log.</param>
        public AnnouncementConnectionsTracker(AnnouncementConnectionsTrackerOptions options, ILog log) : base(options, log)
        {
            this.options = options;
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
            if (e.EndpointDiscoveryMetadata.Scopes.Contains(options.Scope))
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