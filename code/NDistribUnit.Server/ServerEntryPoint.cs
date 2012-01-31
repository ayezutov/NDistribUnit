using System;
using System.Configuration;
using System.Diagnostics;
using Autofac;
using Autofac.Core;
using NDistribUnit.Agent;
using NDistribUnit.Common.Dependencies;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Server;

namespace NDistribUnit.Server
{
    /// <summary>
    /// The entry point for server
    /// </summary>
    internal class ServerEntryPoint: EntryPoint
    {
        /// <summary>
        /// The entry function.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        protected static int Main(string[] args)
        {
            return new ServerEntryPoint().Run(args);
        }

        protected override int RunProgram(string[] args)
        {
            try
            {
                var builder = new ContainerBuilder();
                Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var serverSettings = configuration.GetSection("settings") as ServerConfiguration;
                Debug.Assert(serverSettings != null);
                builder.RegisterType<ServerProgram>();
                builder.RegisterModule(new ServerDependenciesModule(serverSettings, configuration));
                IContainer container = builder.Build();
                return container.Resolve<ServerProgram>().Run();
            }
            catch (DependencyResolutionException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                var log = new ConsoleLog();
                log.Error("Error while running program", ex);
                throw;
            }
        }
    }
}