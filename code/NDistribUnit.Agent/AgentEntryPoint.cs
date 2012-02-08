using System;
using System.Configuration;
using Autofac;
using NDistribUnit.Common.Agent;
using NDistribUnit.Common.Dependencies;
using NDistribUnit.Common.Logging;

namespace NDistribUnit.Agent
{
    /// <summary>
    /// The entry point for agent
    /// </summary>
    public class AgentEntryPoint : EntryPoint
    {
        [STAThread]
        private static int Main(string[] args)
        {
            return new AgentEntryPoint().Run(args);
        }

        /// <summary>
        /// Runs the program specific code.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        protected override int RunProgram(string[] args)
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var agentConfiguration = configuration.GetSection("settings") as AgentConfiguration;
            

            var builder = new ContainerBuilder();
            builder.RegisterType<AgentProgram>();
            builder.RegisterModule(new AgentDependenciesModule(agentConfiguration, args));
            var container = builder.Build();
            try
            {
                return container.Resolve<AgentProgram>().Run();
            }
            catch (Exception ex)
            {
                container.Resolve<ConsoleLog>().Error("Error while running agent", ex);
                throw;
            }
        }
    }
}