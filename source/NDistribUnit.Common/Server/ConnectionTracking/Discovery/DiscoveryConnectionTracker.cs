using System;
using System.ServiceModel.Discovery;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Common.Server.ConnectionTracking.Discovery
{
    /// <summary>
    /// A connection tracker, which relies on discovery mechanism
    /// </summary>
    /// <typeparam name="TIEndpoint"></typeparam>
    public class DiscoveryConnectionTracker<TIEndpoint> : NetworkExplorerBase<TIEndpoint> where TIEndpoint : class, IPingable
    {
        private readonly DiscoveryConnectionTrackerOptions options;
        private readonly IConnectionsHostOptions connectionsHostOptions;

        private DiscoveryClient discoveryClient;
        private FindCriteria findCriteria;
        private bool stopped;


        /// <summary>
        /// Initializes a new instance of connections tracker
        /// </summary>
        /// <param name="options">The options for use while discovery</param>
        /// <param name="connectionsHostOptions">The pingable options.</param>
        /// <param name="log">The log.</param>
        /// <param name="connectionProvider">The connection provider.</param>
        public DiscoveryConnectionTracker(DiscoveryConnectionTrackerOptions options, IConnectionsHostOptions connectionsHostOptions, ILog log, IConnectionProvider connectionProvider)
            : base(connectionsHostOptions, log, connectionProvider)
        {
            this.options = options;
            this.connectionsHostOptions = connectionsHostOptions;
            this.log = log;
        }

        /// <summary>
        /// Starts the connections tracker
        /// </summary>
        public override void Start()
        {
            findCriteria = new FindCriteria(typeof(TIEndpoint))
                               {
                                   Scopes = { connectionsHostOptions.Scope },
                                   Duration = TimeSpan.FromMilliseconds(options.DiscoveryIntervalInMiliseconds)
                               };
            discoveryClient = GetInitilizedDisoveryClient();
            log.Info(string.Format("{0}: Starting clients tracking", guid));

            Discover();
            
        }

        private void Discover()
        {
            
                try
                {
                    discoveryClient.FindAsync(findCriteria);
                }
                catch (Exception)
                {
                    log.Warning(string.Format("{0}: Discovery client got faulted", guid));
                }
        }

        private DiscoveryClient GetInitilizedDisoveryClient()
        {
            var client = new DiscoveryClient(new UdpDiscoveryEndpoint());
            client.FindProgressChanged += OnFindProgressChanged;
            client.FindCompleted += OnFindCompleted;
            return  client;
        }

        private void OnFindCompleted(object sender, FindCompletedEventArgs e)
        {
            if (!e.Cancelled && !stopped)
            {
                if (e.Error != null)
                {
                    log.Warning(string.Format("{0}: Discovery client got faulted", guid));
                    discoveryClient.Close();
                    discoveryClient = GetInitilizedDisoveryClient();
                }
                else
                {
                    log.Info(string.Format("{0}: Discovery client finished. Restarting...", guid));
                }
                discoveryClient.FindAsync(findCriteria);
            }
        }



        private void OnFindProgressChanged(object sender, FindProgressChangedEventArgs e)
        {
            var endpoint = e.EndpointDiscoveryMetadata;
            AddEndpointForTracking(endpoint);
        }

        /// <summary>
        /// Stops the connections tracker
        /// </summary>
        public override void Stop()
        {
            stopped = true;
            discoveryClient.Close();

            StopPingingEndpoints();
        }
    }
}