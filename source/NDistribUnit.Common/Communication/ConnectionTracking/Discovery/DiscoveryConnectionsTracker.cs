using System;
using System.ServiceModel.Discovery;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Common.Communication.ConnectionTracking.Discovery
{
    /// <summary>
    /// A connection tracker, which relies on discovery mechanism
    /// </summary>
    /// <typeparam name="TIEndpoint"></typeparam>
    public class DiscoveryConnectionsTracker<TIEndpoint> : ConnectionsTrackerBase<TIEndpoint> where TIEndpoint : IPingable
    {
        private readonly DiscoveryOptions options;

        private DiscoveryClient discoveryClient;
        private FindCriteria findCriteria;
        private bool stopped;


        /// <summary>
        /// Initializes a new instance of connections tracker
        /// </summary>
        /// <param name="options">The options for use while discovery</param>
        /// <param name="log"></param>
        public DiscoveryConnectionsTracker(DiscoveryOptions options, ILog log): base(options, log)
        {
            this.options = options;
            this.log = log;
        }

        /// <summary>
        /// Starts the connections tracker
        /// </summary>
        public override void Start()
        {
            findCriteria = new FindCriteria(typeof(TIEndpoint))
                               {
                                   Scopes = {options.Scope},
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