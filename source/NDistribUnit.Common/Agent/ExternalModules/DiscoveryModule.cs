using System;
using System.ServiceModel.Discovery;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Common.Updating;

namespace NDistribUnit.Common.Agent.ExternalModules
{
    /// <summary>
    /// The module, which adds discovery to agent
    /// </summary>
    public class DiscoveryModule : IAgentExternalModule
    {
        private readonly IVersionProvider versionProvider;
        private readonly Uri scope;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryModule"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="versionProvider">The version provider.</param>
        public DiscoveryModule(AgentConfiguration options, IVersionProvider versionProvider)
        {
            this.versionProvider = versionProvider;
            this.scope = options.Scope;
        }

        /// <summary>
        /// Starts the module for the given host.
        /// </summary>
        /// <param name="host">The host.</param>
        public void Start(AgentHost host)
        {
            var agentTestRunnerDiscoveryBehavior = host.Endpoint.Behaviors.Find<EndpointDiscoveryBehavior>();
            if (agentTestRunnerDiscoveryBehavior == null)
            {
                agentTestRunnerDiscoveryBehavior = new EndpointDiscoveryBehavior();
                host.Endpoint.Behaviors.Add(agentTestRunnerDiscoveryBehavior);
                AgentAdditionalDataManager.Add(agentTestRunnerDiscoveryBehavior.Extensions, host, versionProvider);
            }
            agentTestRunnerDiscoveryBehavior.Scopes.Add(scope);
            

            host.TestRunnerHost.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
            host.TestRunnerHost.AddServiceEndpoint(new UdpDiscoveryEndpoint());
        }

        /// <summary>
        /// Stops the module for the given host.
        /// </summary>
        /// <param name="host">The host.</param>
        public void Stop(AgentHost host)
        {
        }

        /// <summary>
        /// Aborts the module for the given host.
        /// </summary>
        /// <param name="host">The host.</param>
        public void Abort(AgentHost host)
        {
        }
    }
}