using System;
using Autofac;
using NDistribUnit.Common.Agent;
using NDistribUnit.Common.Agent.ExternalModules;

namespace NDistribUnit.Common.Dependencies
{
    /// <summary>
    /// </summary>
    public class AgentDependenciesModule : Module
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentDependenciesModule"/> class.
        /// </summary>
        /// <param name="agentConfiguration">The agent configuration.</param>
        public AgentDependenciesModule(AgentConfiguration agentConfiguration)
        {
            AgentConfiguration = agentConfiguration;
        }

        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <param name="builder">The builder through which components can be
        /// registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(AgentConfiguration);
            builder.RegisterInstance(AgentConfiguration.LogConfiguration);
            builder.RegisterType<DiscoveryModule>().As<IAgentExternalModule>();
            builder.RegisterType<AnnouncementModule>().As<IAgentExternalModule>();
            builder.RegisterType<TestRunnerAgent>().InstancePerLifetimeScope();
            builder.RegisterType<AgentHost>().InstancePerLifetimeScope();

            builder.RegisterModule(new CommonDependenciesModule(Environment.GetCommandLineArgs()));
        }

        /// <summary>
        /// Gets or sets the agent configuration.
        /// </summary>
        /// <value>
        /// The agent configuration.
        /// </value>
        protected AgentConfiguration AgentConfiguration { get; private set; }
    }
}