using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using NDistribUnit.Common.Communication;
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

        private DashboardService dashboard;
        private TestRunnerServer testRunner;

        public ServerConnectionsTracker ConnectionsTracker { get; private set; }

        /// <summary>
        /// Creates a new server instance, which exposes dashboard and test runner at given ports
        /// </summary>
        /// <param name="dashboardPort">The port for dashboard</param>
        /// <param name="testRunnerPort">The port for test runner</param>
        public ServerHost(int dashboardPort, int testRunnerPort)
        {
            this.dashboardPort = dashboardPort;
            this.testRunnerPort = testRunnerPort;
        }

        /// <summary>
        /// Starts the services for server host
        /// </summary>
        public void Start()
        {
            testRunner = new TestRunnerServer();
            dashboard = new DashboardService(this);

            ExposeDashboardAndTestRunnerAsServices();
            ConnectionsTracker = new ServerConnectionsTracker("http://hubwoo.com/trr-odc");
            ConnectionsTracker.Start();
        }

        /// <summary>
        /// Exposes dashboard and test runner as services
        /// </summary>
        private void ExposeDashboardAndTestRunnerAsServices()
        {
            dashboardService = new ServiceHost(dashboard, new Uri(Path.Combine(string.Format("http://{0}:{1}", Environment.MachineName, dashboardPort))));
            dashboardService.AddServiceEndpoint(typeof(IDashboardService), new WebHttpBinding(), "")
                .Behaviors.Add(new WebHttpBehavior(){DefaultOutgoingResponseFormat = WebMessageFormat.Json});
            dashboardService.Open();

            testRunnerService = new ServiceHost(testRunner, new Uri(string.Format("net.tcp://{0}:{1}", Environment.MachineName, testRunnerPort)));
            testRunnerService.AddServiceEndpoint(typeof(ITestRunnerServer), new NetTcpBinding(), "");
            testRunnerService.Open();
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
    }
}