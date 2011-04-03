using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.ServiceModel.Web;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Server.Services;

namespace NDistribUnit.Server.Communication
{
    /// <summary>
    /// This class manages all the service-related aspects of the NDistribUnit server
    /// </summary>
    public class ServerHost
    {
        private readonly int dashboardPort;
        private readonly int testRunnerPort;

        private ServiceHost dashboardService;
        private ServiceHost testRunnerService;

        private readonly DashboardService dashboard;
        private readonly ILog log;
        private readonly TestRunnerServer testRunner;

        /// <summary>
        /// Gets or sets the connections tracker.
        /// </summary>
        /// <value>
        /// The connections tracker.
        /// </value>
        public ServerConnectionsTracker ConnectionsTracker { get; private set; }

        /// <summary>
        /// Creates a new server instance, which exposes dashboard and test runner at given ports
        /// </summary>
        /// <param name="dashboardPort">The port for dashboard</param>
        /// <param name="testRunnerPort">The port for test runner</param>
        /// <param name="testRunner">The test runner instance</param>
        /// <param name="dashboard">The dashboard instance</param>
        /// <param name="conenctionsTracker">The connection tracker for the host</param>
        /// <param name="log">The logger</param>
        public ServerHost(int dashboardPort, int testRunnerPort, TestRunnerServer testRunner, DashboardService dashboard, ServerConnectionsTracker conenctionsTracker, ILog log)
        {
            this.dashboardPort = dashboardPort;
            this.testRunnerPort = testRunnerPort;
            this.testRunner = testRunner;
            this.dashboard = dashboard;
            this.log = log;
            ConnectionsTracker = conenctionsTracker;
        }

        /// <summary>
        /// Starts the services for server host
        /// </summary>
        public void Start()
        {
            ExposeDashboard();
            ExposeTestRunner();
            ConnectionsTracker.Start();
//            for (int i = 0; i < 10; i++)
//            {
//                var announcementClient = new AnnouncementClient(new UdpAnnouncementEndpoint());
//                foreach (ServiceEndpoint endpoint in testRunnerService.Description.Endpoints)
//                {
//                    EndpointDiscoveryMetadata metadata = EndpointDiscoveryMetadata.
//                        FromServiceEndpoint(endpoint);
//                    try
//                    {
//                        announcementClient.AnnounceOnline(metadata);
//                        log.Success("Successfully announced!");
//                    }
//                    catch(Exception ex)
//                    {
//                        log.Error("Announcing error!", ex);
//                    }
//                }
//            }
        }

        private void ExposeTestRunner()
        {
            testRunnerService = new ServiceHost(testRunner, new Uri(string.Format("net.tcp://{0}:{1}", Environment.MachineName, testRunnerPort)));
            testRunnerService.AddServiceEndpoint(typeof(ITestRunnerServer), new NetTcpBinding(), "");
            testRunnerService.Open();
        }

        private void ExposeDashboard()
        {
            dashboardService = new ServiceHost(dashboard, new Uri(Path.Combine(string.Format("http://{0}:{1}", Environment.MachineName, dashboardPort))));
            dashboardService.AddServiceEndpoint(typeof(IDashboardService), new WebHttpBinding(), "")
                .Behaviors.Add(new WebHttpBehavior(){DefaultOutgoingResponseFormat = WebMessageFormat.Json, DefaultOutgoingRequestFormat = WebMessageFormat.Json});
            dashboardService.Open();
        }

        /// <summary>
        /// Closes all the services, which are exposed
        /// </summary>
        public void Close()
        {
            ConnectionsTracker.Stop();
            dashboardService.Close();
            testRunnerService.Close();
        }

        /// <summary>
        /// Aborts this instance.
        /// </summary>
        public void Abort()
        {
            ConnectionsTracker.Stop();
            dashboardService.Abort();
            testRunnerService.Abort();
        }
    }
}