using System;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Server;

namespace NDistribUnit.Agent.Communication
{
    public class AgentHost
    {
        protected ServiceHost AgentTestRunnerHost { get; set; }

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

        public void Stop()
        {
            AgentTestRunnerHost.Close();
        }
    }
}