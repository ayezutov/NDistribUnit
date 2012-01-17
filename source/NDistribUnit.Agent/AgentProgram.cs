using System;
using System.Configuration;
using Autofac;
using NDistribUnit.Common.Agent;
using NDistribUnit.Common.Agent.Naming;
using NDistribUnit.Common.Common.ConsoleProcessing;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.Dependencies;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Agent
{
    /// <summary>
    /// The entry point into agent's console application
    /// </summary>
    public class AgentProgram : GeneralProgram
    {
        [STAThread]
        private static int Main(string[] args)
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var agentConfiguration = configuration.GetSection("settings") as AgentConfiguration;
            if (agentConfiguration != null)
                agentConfiguration.AgentName = string.Format("{0} #{1:000}", Environment.MachineName,
                                                         InstanceNumberSearcher.Number);

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

        private readonly BootstrapperParameters bootstrapperParameters;
        private readonly ILog log;

        /// <summary>
        ///  The host, which enables all communication services
        /// </summary>
        public AgentHost AgentHost { get; set; }


        /// <summary>
        /// Initializes a new instance of an agent program
        /// </summary>
        /// <param name="agentHost">The agent host.</param>
        /// <param name="bootstrapperParameters">The bootstrapper parameters.</param>
        /// <param name="updatesMonitor">The updates availability monitor.</param>
        /// <param name="exceptionCatcher">The exception catcher.</param>
        /// <param name="log">The log.</param>
        public AgentProgram(AgentHost agentHost, BootstrapperParameters bootstrapperParameters,
                            UpdatesMonitor updatesMonitor, ExceptionCatcher exceptionCatcher, ILog log)
        {
            this.bootstrapperParameters = bootstrapperParameters;
            this.updatesMonitor = updatesMonitor;
            this.exceptionCatcher = exceptionCatcher;
            this.log = log;
            AgentHost = agentHost;
        }


        private int Run()
        {
            return exceptionCatcher.Run(
                () =>
                {
                    if (!bootstrapperParameters.AllParametersAreFilled)
                    {
                        log.Error("This programm cannot be launched directly");
                        return (int)ReturnCodes.CannotLaunchBootstrappedApplicationDirectly;
                    }

                    //AgentHost.LoadState();
                    updatesMonitor.Start();

                    try
                    {
                        log.BeginActivity("Starting agent...");
                        AgentHost.Start();
                        log.EndActivity(
                            "Agent was successfully started. Press <Enter> to exit.");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Exception, while running", ex);
                        throw;
                    }


                    var returnCode = WaitAndGetReturnCode(new string[0]);

                    updatesMonitor.Stop();
                    // AgentHost.SaveState();
                    AgentHost.Stop();


                    return returnCode;
                });
        }


    }

}