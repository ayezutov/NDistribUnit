using System.Configuration;
using Autofac;
using NDistribUnit.Common.Common.Logging;
using NDistribUnit.Common.Communication.ConnectionTracking;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Server;
using NDistribUnit.Common.Server.Communication;
using NDistribUnit.Common.Server.ConnectionTracking;
using NDistribUnit.Common.Server.Services;
using NDistribUnit.Common.TestExecution;
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
        /// <param name="commandLineArgs">The command line args.</param>
        /// <param name="configuration">The configuration.</param>
        public ServerDependenciesModule(ServerConfiguration serverConfiguration, string[] commandLineArgs, Configuration configuration = null)
        {
            ServerConfiguration = serverConfiguration;
            CommandLineArgs = commandLineArgs;
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
        /// Gets or sets the args.
        /// </summary>
        /// <value>
        /// The args.
        /// </value>
        public string[] CommandLineArgs { get; set; }

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

            builder.RegisterType<TestRunnerServer>().InstancePerLifetimeScope();
            builder.RegisterType<TestUnitCollection>().InstancePerLifetimeScope();
            builder.RegisterType<DashboardService>().InstancePerLifetimeScope();
            builder.RegisterType<ServerConnectionsTracker>().InstancePerLifetimeScope();
            builder.RegisterType<UpdateSource>().As<IUpdateSource>();
            builder.RegisterType<RequestsStorage>().As<IRequestsStorage>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<TestsRetriever>().As<ITestsRetriever>();
            builder.RegisterType<TestsScheduler>().As<ITestsScheduler>().InstancePerLifetimeScope();
            builder.RegisterType<TestManager>().InstancePerLifetimeScope();
            builder.RegisterType<ServerHost>().InstancePerLifetimeScope();
            builder.Register(c => new WindowsLog("Server")).InstancePerLifetimeScope(); ;
            builder.RegisterModule(new CommonDependenciesModule(CommandLineArgs));


            foreach (ConnectionTrackerElement connectionTracker in ServerConfiguration.ConnectionTrackers)
            {
                builder.RegisterType(connectionTracker.Type).As<INetworkExplorer<ITestRunnerAgent>>();
                if (!string.IsNullOrEmpty(connectionTracker.SectionName))
                {
                    ConfigurationSection section = Configuration.GetSection(connectionTracker.SectionName);
                    builder.RegisterInstance(section).As(section.GetType());
                }
            }
        }
    }
}