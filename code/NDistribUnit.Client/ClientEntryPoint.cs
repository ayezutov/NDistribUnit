using System;
using Autofac;
using NDistribUnit.Agent;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Common.Logging;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.Dependencies;
using NDistribUnit.Common.Logging;

namespace NDistribUnit.Client
{
    /// <summary>
    /// The entry point for client
    /// </summary>
    public class ClientEntryPoint : EntryPoint
    {
        /// <summary>
        /// The entry function.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        static int Main(string[] args)
        {
            return new ClientEntryPoint().Run(args);
        }

        /// <summary>
        /// Runs the program specific code.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        protected override int RunProgram(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.Register(c => ClientParameters.Parse(args)).InstancePerLifetimeScope();
            builder.Register(c => new LogConfiguration { RollingLogItemsCount = 1000 }).InstancePerLifetimeScope();
            builder.RegisterType<ClientProgram>();
            builder.RegisterModule(new ClientDependenciesModule());
            builder.RegisterModule(new CommonDependenciesModule());
            var container = builder.Build();
            try
            {
                return container.Resolve<ClientProgram>().Run();
            }
            catch (Exception ex)
            {
                var log = container.Resolve<ConsoleLog>();
                log.Error("Some error, while running tests", ex);
                return (int)ReturnCodes.UnhandledException;
            }
        }
    }
}