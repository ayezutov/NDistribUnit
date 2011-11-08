using Autofac;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.TestExecution.Configuration;

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
            builder.Register(c => new WindowsLog("Client")).InstancePerLifetimeScope();
            builder
                .RegisterType<TestRunnerClient>()
                .As<ITestRunnerClient>().AsSelf();
            builder
                .RegisterType<TestRunParametersFileReader>()
                .As<ITestRunParametersFileReader>();
        }
    }
}