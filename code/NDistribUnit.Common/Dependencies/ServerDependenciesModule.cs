using System.Configuration;
using Autofac;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Server;
using NDistribUnit.Common.Server.AgentsTracking;
using NDistribUnit.Common.Server.AgentsTracking.AgentsProviders;
using NDistribUnit.Common.Server.Communication;
using NDistribUnit.Common.Server.Services;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.TestExecution.DistributedConfiguration;
using NDistribUnit.Common.TestExecution.Scheduling;
using NDistribUnit.Common.TestExecution.Storage;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Common.Dependencies
{
    /// <summary>
    /// </summary>
    public class ServerDependenciesModule : Module
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerDependenciesModule"/> class.
        /// </summary>
        /// <param name="serverConfiguration">The server configuration.</param>
        /// <param name="configuration">The configuration.</param>
        public ServerDependenciesModule(ServerConfiguration serverConfiguration, Configuration configuration = null)
        {
            ServerConfiguration = serverConfiguration;
            Configuration = configuration;
        }

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public ServerConfiguration ServerConfiguration { get; set; }

        
        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public Configuration Configuration { get; set; }

        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <param name="builder">The builder through which components can be
        /// registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(ServerConfiguration)
                .As<IConnectionsHostOptions>()
                .AsSelf();
            builder.RegisterInstance(ServerConfiguration.LogConfiguration);

            builder.RegisterType<Server.Services.Server>().InstancePerLifetimeScope();
            builder.RegisterType<TestUnitsCollection>().AsSelf().As<ITestUnitsCollection>().InstancePerLifetimeScope();
            builder.RegisterType<AgentsCollection>().InstancePerLifetimeScope();
            builder.RegisterType<AgentUpdater>().As<IAgentUpdater>().InstancePerLifetimeScope();
            builder.RegisterType<DashboardService>().InstancePerLifetimeScope();
            builder.RegisterType<AgentsTracker>().InstancePerLifetimeScope();
            builder.RegisterType<UpdateSource>().As<IUpdateSource>();
            builder.RegisterType<RequestsStorage>().As<IRequestsStorage>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<TestReprocessor>().As<ITestReprocessor>().InstancePerLifetimeScope();
            builder.Register(c => 
                new ResultsStorage(c.Resolve<TestResultsProcessor>(), 
                    c.Resolve<ITestResultsSerializer>(),
                    c.Resolve<ILog>(), 
                    c.Resolve<BootstrapperParameters>(),
                    "Server.Results")).As<IResultsStorage>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<TestsRetriever>().As<ITestsRetriever>();
            builder.RegisterType<TestsScheduler>().As<ITestsScheduler>().InstancePerLifetimeScope();
            builder.RegisterType<ServerTestRunner>().InstancePerLifetimeScope();
            builder.RegisterType<ServerHost>().InstancePerLifetimeScope();
            builder.Register(c => new WindowsLog("Server")).InstancePerLifetimeScope();
            builder.RegisterModule(new CommonDependenciesModule());
            builder.Register(c=> new ProjectsStorage("Server", c.Resolve<BootstrapperParameters>(), c.Resolve<ZipSource>(), c.Resolve<ILog>())).As<IProjectsStorage>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<DistributedConfigurationOperator>().As<IDistributedConfigurationOperator>();

            foreach (ConnectionTrackerElement connectionTracker in ServerConfiguration.ConnectionTrackers)
            {
                builder.RegisterType(connectionTracker.Type).As<IAgentsProvider>();
                if (!string.IsNullOrEmpty(connectionTracker.SectionName))
                {
                    ConfigurationSection section = Configuration.GetSection(connectionTracker.SectionName);
                    builder.RegisterInstance(section).As(section.GetType());
                }
            }
        }
    }
}