using System;
using System.Diagnostics;
using System.ServiceModel.Discovery;
using System.Threading;
using NDistribUnit.Common;

namespace NDistribUnit.Agent.Communication.ExternalModules
{
    /// <summary>
    /// Adds additional functionality for enabling discovery through announcement
    /// </summary>
    public class AnnouncementModule: IAgentExternalModule
    {
        private AnnouncementClient client;
        private TimeSpan defaultPingInterval;
        private readonly Uri scope;
        private Timer announcementTimer;
        private EndpointDiscoveryMetadata endpointDiscoveryMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnouncementModule"/> class.
        /// </summary>
        /// <param name="defaultPingInterval">The default ping interval.</param>
        /// <param name="scope"></param>
        public AnnouncementModule(TimeSpan defaultPingInterval, Uri scope)
        {
            this.defaultPingInterval = defaultPingInterval;
            this.scope = scope;
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

            Announce();
        }

        private void PingReceived(object sender, EventArgs<TimeSpan> eventArgs)
        {
            lock (this)
            {
                var timeoutBeforeAnnouncement = eventArgs.Data.Add(TimeSpan.FromSeconds(2));
                if (timeoutBeforeAnnouncement < defaultPingInterval)
                    timeoutBeforeAnnouncement = defaultPingInterval;

                announcementTimer.Change(timeoutBeforeAnnouncement.Milliseconds, Timeout.Infinite);
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
                    client.AnnounceOnline(endpointDiscoveryMetadata);
                    announcementTimer.Change(defaultPingInterval.Milliseconds, Timeout.Infinite);
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
                announcementTimer.Change(Timeout.Infinite, Timeout.Infinite);
                announcementTimer = null;
                client.Close();
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
                announcementTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
    }
}