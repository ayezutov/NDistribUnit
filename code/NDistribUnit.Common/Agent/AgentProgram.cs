using System;
using NDistribUnit.Common.Common.ConsoleProcessing;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Common.Agent
{
    /// <summary>
    /// The entry point into agent's console application
    /// </summary>
    public class AgentProgram : GeneralProgram
    {
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
                            UpdatesMonitor updatesMonitor, ExceptionCatcher exceptionCatcher,
                            ILog log) : base(updatesMonitor, exceptionCatcher, log, bootstrapperParameters)
        {
            AgentHost = agentHost;
        }


        /// <summary>
        /// Runs this instance.
        /// </summary>
        /// <returns></returns>
        public int Run()
        {
            return exceptionCatcher.Run(
                () =>
                    {
                        //AgentHost.LoadState();
                        updatesMonitor.Start();

                        try
                        {
                            log.BeginActivity("Starting agent...");
                            AgentHost.Start();
                            log.EndActivity("Agent was successfully started. Please type \"exit\" and press <Enter> to exit.");
                        }
                        catch (Exception ex)
                        {
                            log.Error("Exception, while starting agent", ex);
                            throw;
                        }

                        var returnCode = WaitAndGetReturnCode(new string[0] /*{bootstrapperParameters.ConfigurationFile}*/);

                        updatesMonitor.Stop();
                        // AgentHost.SaveState();
                        AgentHost.Stop();


                        return returnCode;
                    });
        }
    }
}