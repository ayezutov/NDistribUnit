using System;
using System.ServiceModel;
using System.ServiceModel.Description;
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
        private Uri scope;
        private ServiceHost AgentTestRunnerHost { get; set; }

        /// <summary>
        /// Gets the endpoint.
        /// </summary>
        public ServiceEndpoint Endpoint { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentHost"/> class.
        /// </summary>
        /// <param name="scope">The scope.</param>
        public AgentHost(Uri scope)
        {
            this.scope = scope;
        }

        /// <summary>
        /// Starts agent's services
        /// </summary>
        public void Start()
        {
            var testRunnerAgent = new TestRunnerAgent();

            AgentTestRunnerHost = new ServiceHost(testRunnerAgent, new Uri(string.Format("net.tcp://{0}:{1}", Environment.MachineName, WcfUtilities.FindPort())));
            Endpoint = AgentTestRunnerHost.AddServiceEndpoint(typeof (ITestRunnerAgent), new NetTcpBinding(), "");
            var agentTestRunnerDiscoveryBehavior = new EndpointDiscoveryBehavior();
            agentTestRunnerDiscoveryBehavior.Scopes.Add(scope);
            Endpoint.Behaviors.Add(agentTestRunnerDiscoveryBehavior);
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

        /// <summary>
        /// Aborts this instance.
        /// </summary>
        public void Abort()
        {
            AgentTestRunnerHost.Abort();
        }
    }
}