using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel.Discovery;
using System.Threading;
using System.Xml.Linq;
using NDistribUnit.Common.Communication.ConnectionTracking;
using NDistribUnit.Common.Logging;

namespace NDistribUnit.Common.Agent.ExternalModules
{
    /// <summary>
    /// Adds additional functionality for enabling discovery through announcement
    /// </summary>
    public class AnnouncementModule: IAgentExternalModule
    {
        private AnnouncementClient client;
        private TimeSpan defaultPingInterval;
        private readonly Uri scope;
        private readonly ILog log;
        private Timer announcementTimer;
        private EndpointDiscoveryMetadata endpointDiscoveryMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnouncementModule"/> class.
        /// </summary>
        /// <param name="defaultPingInterval">The default ping interval.</param>
        /// <param name="scope"></param>
        /// <param name="log"></param>
        public AnnouncementModule(TimeSpan defaultPingInterval, Uri scope, ILog log)
        {
            this.defaultPingInterval = defaultPingInterval;
            this.scope = scope;
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
            AgentAdditionalDataManager.Add(endpointDiscoveryMetadata.Extensions, host);
            Announce();
        }

        private void PingReceived(object sender, EventArgs<TimeSpan> eventArgs)
        {
            lock (this)
            {
                log.Debug(string.Format("Ping received: {0}, Ping interval: {1}", endpointDiscoveryMetadata.Address, eventArgs.Data));
                var timeoutBeforeAnnouncement = eventArgs.Data.Add(TimeSpan.FromSeconds(2));
                if (timeoutBeforeAnnouncement < defaultPingInterval)
                    timeoutBeforeAnnouncement = defaultPingInterval;

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

                    announcementTimer.Change((int)defaultPingInterval.TotalMilliseconds, Timeout.Infinite);
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