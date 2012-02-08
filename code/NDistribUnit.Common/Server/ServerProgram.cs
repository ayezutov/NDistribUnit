using System;
using NDistribUnit.Common.Common.ConsoleProcessing;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Server.Communication;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Common.Server
{
    /// <summary>
    /// The server specific steps for running program 
    /// </summary>
    public class ServerProgram : GeneralProgram
    {
        private readonly ServerHost serverHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerProgram"/> class.
        /// </summary>
        /// <param name="serverHost">The server host.</param>
        /// <param name="bootstrapperParameters">The bootstrapper parameters.</param>
        /// <param name="updatesMonitor">The updates monitor.</param>
        /// <param name="log">The log.</param>
        /// <param name="exceptionCatcher">The exception catcher.</param>
        public ServerProgram(ServerHost serverHost, BootstrapperParameters bootstrapperParameters,
                             UpdatesMonitor updatesMonitor, ILog log, ExceptionCatcher exceptionCatcher)
            :base(updatesMonitor, exceptionCatcher, log, bootstrapperParameters)
        {
            this.serverHost = serverHost;
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        /// <returns></returns>
        public int Run()
        {
            log.Error("Is it available", new NotImplementedException("He?"));
            return exceptionCatcher.Run(() =>
            {
                if (!bootstrapperParameters.AllParametersAreFilled)
                {
                    log.Error("This program cannot be launched directly");
                    return (int)ReturnCodes.CannotLaunchBootstrappedApplicationDirectly;
                }
                //serverHost.LoadState();

                updatesMonitor.Start();

                log.BeginActivity("Server is starting...");

                serverHost.Start();
                log.EndActivity(@"Server was started. Please type ""exit"" and press <Enter> to exit");

                var returnCode = WaitAndGetReturnCode(new[] { bootstrapperParameters.ConfigurationFile });

                //serverHost.SaveState();
                updatesMonitor.Stop();
                serverHost.Close();

                return returnCode;
            });
        }
    }
}