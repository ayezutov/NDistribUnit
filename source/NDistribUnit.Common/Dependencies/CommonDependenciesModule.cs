using System;
using Autofac;
using NDistribUnit.Common.Common.Logging;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.Logging;
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
            builder.RegisterType<UpdateReceiver>();
            builder.RegisterType<ZipSource>();
            builder.Register(c => new RollingLog(c.Resolve<LogConfiguration>().RollingLogItemsCount));
            builder.RegisterType<ConsoleLog>();
            builder.Register(
                c => new CombinedLog(c.Resolve<ConsoleLog>(), c.Resolve<RollingLog>()))
                .As<ILog>()
                .SingleInstance();
        }
    }
}