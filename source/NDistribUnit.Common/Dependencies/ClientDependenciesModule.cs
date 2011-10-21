using Autofac;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Common.Dependencies
{
    /// <summary>
    /// 
    /// </summary>
    public class ClientDependenciesModule: Module
    {
        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <param name="builder">The builder through which components can be
        /// registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<UpdateReceiver>();
            builder.RegisterType<VersionDirectoryFinder>();
            builder.RegisterType<ConsoleLog>().As<ILog>();
        }
    }
}