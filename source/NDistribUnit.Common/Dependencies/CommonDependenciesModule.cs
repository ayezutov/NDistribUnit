using System;
using Autofac;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Common.Logging;
using NDistribUnit.Common.Common.Networking;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.TestExecution.Storage;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Common.Dependencies
{
    /// <summary>
    /// </summary>
    public class CommonDependenciesModule : Module
    {
        private readonly string[] commandLineArgs;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonDependenciesModule"/> class.
        /// </summary>
        /// <param name="commandLineArgs">The command line args.</param>
        public CommonDependenciesModule(string[] commandLineArgs)
        {
            this.commandLineArgs = commandLineArgs;
        }

        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <param name="builder">The builder through which components can be
        /// registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => BootstrapperParameters.Parse(commandLineArgs))
                .SingleInstance();
            builder.RegisterType<UpdatesMonitor>();
            builder.RegisterType<VersionDirectoryFinder>();
            builder.RegisterType<CurrentAssemblyVersionProvider>().As<IVersionProvider>();
            builder.RegisterType<UpdateReceiver>().As<IUpdateReceiver>().AsSelf();
            builder.RegisterType<ZipSource>();
            builder.RegisterType<ProjectPackager>().As<IProjectPackager>();
            builder.RegisterType<TestResultsProcessor>();
            builder.RegisterType<TestResultsSerializer>().As<ITestResultsSerializer>().InstancePerLifetimeScope();

            builder.Register(c => new RollingLog(c.Resolve<LogConfiguration>().RollingLogItemsCount)).InstancePerLifetimeScope();
            builder.RegisterType<ConsoleLog>().InstancePerLifetimeScope();
            builder.Register(
                c => new CombinedLog(c.Resolve<ConsoleLog>(), c.Resolve<RollingLog>(), c.Resolve<WindowsLog>()))
                .As<ILog>()
                .SingleInstance();
            builder.RegisterType<RealConnectionProvider>().As<IConnectionProvider>().InstancePerLifetimeScope();

            
            builder.RegisterType<NUnitInitializer>().As<ITestSystemInitializer>().InstancePerLifetimeScope();
        }
    }
}