using System;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Extensions;

namespace NDistribUnit.Common.Server.AgentsTracking.AgentsProviders.Discovery
{
    /// <summary>
    /// A provider, which uses WCF discovery. It issues a broadcast message, searching for all agents,
    /// and waits for answers from them.
    /// </summary>
    public class DiscoveryAgentsProvider : IAgentsProvider
    {
        private readonly DiscoveryAgentsProviderOptions options;
        private readonly IConnectionsHostOptions connectionsHostOptions;
        private readonly ILog log;
        private FindCriteria findCriteria;
        private DiscoveryClient discoveryClient;
        private bool stopped;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryAgentsProvider"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="connectionsHostOptions">The connections host options.</param>
        /// <param name="log">The log.</param>
        public DiscoveryAgentsProvider(DiscoveryAgentsProviderOptions options, IConnectionsHostOptions connectionsHostOptions, ILog log)
        {
            this.options = options;
            this.connectionsHostOptions = connectionsHostOptions;
            this.log = log;
        }

        /// <summary>
        /// Occurs when an agent is connected.
        /// </summary>
        public event EventHandler<EventArgs<EndpointAddress>> Connected;

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            findCriteria = new FindCriteria(typeof(IAgent))
            {
                Scopes = { connectionsHostOptions.Scope },
                Duration = TimeSpan.FromMilliseconds(options.DiscoveryIntervalInMiliseconds)
            };
            discoveryClient = GetInitilizedDisoveryClient();
            log.Info("Starting clients tracking");

            Discover();
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            stopped = true;
            discoveryClient.Close();
        }

        private void Discover()
        {

            try
            {
                discoveryClient.FindAsync(findCriteria);
            }
            catch (Exception)
            {
                log.Warning("Discovery client got faulted");
            }
        }

        private DiscoveryClient GetInitilizedDisoveryClient()
        {
            var client = new DiscoveryClient(new UdpDiscoveryEndpoint());
            client.FindProgressChanged += OnFindProgressChanged;
            client.FindCompleted += OnFindCompleted;
            return client;
        }

        private void OnFindCompleted(object sender, FindCompletedEventArgs e)
        {
            if (!e.Cancelled && !stopped)
            {
                if (e.Error != null)
                {
                    log.Warning("Discovery client got faulted");
                    discoveryClient.Close();
                    discoveryClient = GetInitilizedDisoveryClient();
                }
                else
                {
                    log.Info("Discovery client finished. Restarting...");
                }
                discoveryClient.FindAsync(findCriteria);
            }
        }

        private void OnFindProgressChanged(object sender, FindProgressChangedEventArgs e)
        {
            Connected.SafeInvoke(this, e.EndpointDiscoveryMetadata.Address);
        }
    }
}