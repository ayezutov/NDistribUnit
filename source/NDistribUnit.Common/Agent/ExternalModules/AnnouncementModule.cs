using System;
using System.Diagnostics;
using System.ServiceModel.Discovery;
using System.Threading;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Logging;

namespace NDistribUnit.Common.Agent.ExternalModules
{
    /// <summary>
    /// Adds additional functionality for enabling discovery through announcement
    /// </summary>
    public class AnnouncementModule: IAgentExternalModule
    {
        private AnnouncementClient client;
        private TimeSpan announcementInterval;
        private readonly Uri scope;
        private readonly ILog log;
        private Timer announcementTimer;
        private EndpointDiscoveryMetadata endpointDiscoveryMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnouncementModule"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="log">The log.</param>
        public AnnouncementModule(AgentConfiguration options, ILog log)
        {
            announcementInterval = options.AnnouncementInterval;
            scope = options.Scope;
            this.log = log;
            announcementTimer = new Timer(o=>Announce(), null, Timeout.Infinite, Timeout.Infinite);
        }


        /// <summary>
        /// Starts the module for the given host.
        /// </summary>
        /// <param name="host">The host.</param>
        public void Start(AgentHost host)
        {
            client = new AnnouncementClient(new UdpAnnouncementEndpoint());
            client.Open();
            host.TestRunner.PingReceived += PingReceived;
            
            endpointDiscoveryMetadata = EndpointDiscoveryMetadata.FromServiceEndpoint(host.Endpoint);

            Debug.Assert(endpointDiscoveryMetadata != null);

            if (!endpointDiscoveryMetadata.Scopes.Contains(scope))
                endpointDiscoveryMetadata.Scopes.Add(scope);
            AdditionalDataManager.Add(endpointDiscoveryMetadata.Extensions, host);
            Announce();
        }

        private void PingReceived(object sender, EventArgs<TimeSpan> eventArgs)
        {
            lock (this)
            {
                log.Debug(string.Format("Ping received: {0}, Ping interval: {1}", endpointDiscoveryMetadata.Address, eventArgs.Data));
                var timeoutBeforeAnnouncement = eventArgs.Data.Add(TimeSpan.FromSeconds(2));
                if (timeoutBeforeAnnouncement < announcementInterval)
                    timeoutBeforeAnnouncement = announcementInterval;

                announcementTimer.Change((int)timeoutBeforeAnnouncement.TotalMilliseconds, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Announces the hosts' endpoint.
        /// </summary>
        private void Announce()
        {
            lock (this)
            {
                if (announcementTimer != null)
                {
                    log.Info(string.Format("Announcing endpoint {0}", endpointDiscoveryMetadata.Address));
                    client.AnnounceOnline(endpointDiscoveryMetadata);

                    announcementTimer.Change((int)announcementInterval.TotalMilliseconds, Timeout.Infinite);
                }
            }
        }

        /// <summary>
        /// Stops the module for the given host.
        /// </summary>
        /// <param name="host">The host.</param>
        public void Stop(AgentHost host)
        {
            lock (this)
            {
                client.Close();
                if (announcementTimer != null)
                {
                    announcementTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    announcementTimer = null;
                }
            }
        }

        /// <summary>
        /// Aborts the module for the given host.
        /// </summary>
        /// <param name="host">The host.</param>
        public void Abort(AgentHost host)
        {
            lock (this)
            {
                client.Close();
                if (announcementTimer != null)
                    announcementTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
    }
}