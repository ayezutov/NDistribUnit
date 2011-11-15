using System;
using Autofac;
using NDistribUnit.Common.Agent;
using NDistribUnit.Common.Agent.ExternalModules;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.TestExecution.Storage;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Common.Dependencies
{
    /// <summary>
    /// </summary>
    public class AgentDependenciesModule : Module
    {
        private readonly string[] args;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentDependenciesModule"/> class.
        /// </summary>
        /// <param name="agentConfiguration">The agent configuration.</param>
        /// <param name="args"></param>
        public AgentDependenciesModule(AgentConfiguration agentConfiguration, string[] args)
        {
            this.args = args;
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
            builder.RegisterType<Agent.Agent>().InstancePerLifetimeScope();
            builder.RegisterType<AgentHost>().InstancePerLifetimeScope();
            builder.RegisterType<AgentTestRunner>().InstancePerLifetimeScope();
            builder.RegisterType<NativeRunnerCache>().As<INativeRunnerCache>().InstancePerLifetimeScope();
            builder.Register(c => new WindowsLog("Agent")).InstancePerLifetimeScope();
            builder.Register(c => new ProjectsStorage("Agent", c.Resolve<BootstrapperParameters>(), c.Resolve<ZipSource>())).As<IProjectsStorage>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterModule(new CommonDependenciesModule(args));
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