using System;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Agent.Communication
{
    /// <summary>
    /// A host, which manages all agent's services
    /// </summary>
    public class AgentHost
    {
        private ServiceHost AgentTestRunnerHost { get; set; }

        /// <summary>
        /// Starts agent's services
        /// </summary>
        public void Start()
        {
            var testRunnerAgent = new TestRunnerAgent();

            AgentTestRunnerHost = new ServiceHost(testRunnerAgent, new Uri(string.Format("net.tcp://{0}:{1}", Environment.MachineName, WcfUtilities.FindPort())));
            var agentTestRunnerEndpoint = AgentTestRunnerHost.AddServiceEndpoint(typeof (ITestRunnerAgent), new NetTcpBinding(), "");
            var agentTestRunnerDiscoveryBehavior = new EndpointDiscoveryBehavior();
            agentTestRunnerDiscoveryBehavior.Scopes.Add(new Uri("http://hubwoo.com/trr-odc"));
            agentTestRunnerEndpoint.Behaviors.Add(agentTestRunnerDiscoveryBehavior);
            AgentTestRunnerHost.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
            AgentTestRunnerHost.AddServiceEndpoint(new UdpDiscoveryEndpoint());
            AgentTestRunnerHost.Open();
        }

        /// <summary>
        /// Stops all agent's services
        /// </summary>
        public void Stop()
        {
            AgentTestRunnerHost.Close();
        }
    }
}