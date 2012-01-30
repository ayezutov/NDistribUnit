using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Threading;
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
        }

        /// <summary>
        /// Starts the module for the given host.
        /// </summary>
        /// <param name="host">The host.</param>
        public void Start(AgentHost host)
        {
            lock (this)
            {
                client = CreateAnnouncementClient();
                client.Open();
                host.TestRunner.PingReceived += PingReceived;
                host.TestRunner.CommunicationStarted += CommunicationStarted;
                host.TestRunner.CommunicationFinished += CommunicationFinished;
                
                endpointDiscoveryMetadata = EndpointDiscoveryMetadata.FromServiceEndpoint(host.Endpoint);
                announcementTimer = new Timer(o => Announce(), null, Timeout.Infinite, Timeout.Infinite);

                Debug.Assert(endpointDiscoveryMetadata != null);

                if (!endpointDiscoveryMetadata.Scopes.Contains(scope))
                    endpointDiscoveryMetadata.Scopes.Add(scope);
                
                new Action(Announce).BeginInvoke(null, null);
            }
        }

        private static AnnouncementClient CreateAnnouncementClient()
        {
            return new AnnouncementClient(new UdpAnnouncementEndpoint());
        }

        private volatile int communicationsCount;
        private void CommunicationFinished(object sender, EventArgs e)
        {
            lock (this)
            {
                communicationsCount--;
                if (communicationsCount == 0)
                    announcementTimer.Change((int)announcementInterval.TotalMilliseconds, Timeout.Infinite);
            }
        }

        private void CommunicationStarted(object sender, EventArgs e)
        {
            lock (this)
            {
                communicationsCount++;
                announcementTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        private void PingReceived(object sender, EventArgs<TimeSpan> eventArgs)
        {
            lock (this)
            {
                log.Debug(string.Format("Ping received: {0}, Ping interval: {1}", endpointDiscoveryMetadata.Address, eventArgs.Data));

                if (communicationsCount > 0)
                    return;

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
                if (announcementTimer != null && communicationsCount==0)
                {
                    try
                    {
                        log.Debug(string.Format("Announcing endpoint {0}", endpointDiscoveryMetadata.Address));
                        if (client.InnerChannel.State == CommunicationState.Faulted)
                        {
                            client.Close();
                            client = CreateAnnouncementClient();
                        }
                        client.AnnounceOnline(endpointDiscoveryMetadata);
                    }
                    catch(Exception ex)
                    {
                        log.Debug(string.Format("There was an exception while announcing: {0}", ex.Message));
                    }
                    finally
                    {
                        announcementTimer.Change((int) announcementInterval.TotalMilliseconds, Timeout.Infinite);
                    }
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
            Stop(host);
        }
    }
}