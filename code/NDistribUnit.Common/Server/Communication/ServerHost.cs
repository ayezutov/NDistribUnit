using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Server.AgentsTracking;
using NDistribUnit.Common.Server.Services;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Common.Server.Communication
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
        private readonly Services.Server testRunner;

        /// <summary>
        /// Gets or sets the connections tracker.
        /// </summary>
        /// <value>
        /// The connections tracker.
        /// </value>
        public AgentsTracker ConnectionsTracker { get; private set; }

        /// <summary>
        /// Creates a new server instance, which exposes dashboard and test runner at given ports
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="testRunner">The test runner instance</param>
        /// <param name="dashboard">The dashboard instance</param>
        /// <param name="connectionsTracker">The connection tracker for the host</param>
        /// <param name="log">The logger</param>
        public ServerHost(ServerConfiguration configuration, Services.Server testRunner, DashboardService dashboard, AgentsTracker connectionsTracker, ILog log)
        {
            dashboardPort = configuration.DashboardPort;
            testRunnerPort = configuration.TestRunnerPort;
            this.testRunner = testRunner;
            this.dashboard = dashboard;
            this.log = log;
            ConnectionsTracker = connectionsTracker;
        }

        /// <summary>
        /// Starts the services for server host
        /// </summary>
        public void Start()
        {
            ExposeDashboard();
            ExposeTestRunner();
            ConnectionsTracker.Start();
        }

        private void ExposeTestRunner()
        {
            var baseAddress = new Uri(string.Format("net.tcp://{0}:{1}", Environment.MachineName, testRunnerPort));
            log.Info(string.Format("Starting server at {0}", baseAddress));
            testRunnerService = new ServiceHost(testRunner, baseAddress);
            testRunnerService.AddServiceEndpoint(typeof(IServer), new NetTcpBinding("NDistribUnit.Default"), "");
            testRunnerService.Open();
        }

        private void ExposeDashboard()
        {
            dashboardService = new ServiceHost(dashboard, new Uri(string.Format("http://{0}:{1}", Environment.MachineName, dashboardPort)));
            dashboardService.AddServiceEndpoint(typeof(IDashboardService), new WebHttpBinding(), "")
                .Behaviors.Add(new WebHttpBehavior {DefaultOutgoingResponseFormat = WebMessageFormat.Json, DefaultOutgoingRequestFormat = WebMessageFormat.Json});
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