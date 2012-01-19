using System;
using System.ServiceModel.Discovery;

namespace NDistribUnit.Common.Agent.ExternalModules
{
    /// <summary>
    /// The module, which adds discovery to agent
    /// </summary>
    public class DiscoveryModule : IAgentExternalModule
    {
        private readonly Uri scope;
        private EndpointDiscoveryBehavior discoveryBehavior;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoveryModule"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public DiscoveryModule(AgentConfiguration options)
        {
            scope = options.Scope;
        }

        /// <summary>
        /// Starts the module for the given host.
        /// </summary>
        /// <param name="host">The host.</param>
        public void Start(AgentHost host)
        {
            discoveryBehavior = host.Endpoint.Behaviors.Find<EndpointDiscoveryBehavior>();
            if (discoveryBehavior == null)
            {
                discoveryBehavior = new EndpointDiscoveryBehavior();
                host.Endpoint.Behaviors.Add(discoveryBehavior);
            }
            discoveryBehavior.Scopes.Add(scope);
            

            host.TestRunnerHost.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
            host.TestRunnerHost.AddServiceEndpoint(new UdpDiscoveryEndpoint());

            host.TestRunner.UpdateStarted += (sender, args) => { discoveryBehavior.Enabled = false; };
            host.TestRunner.UpdateFinished += (sender, args) => { discoveryBehavior.Enabled = true; };
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