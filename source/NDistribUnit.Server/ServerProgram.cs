using System;
using System.Configuration;
using System.Diagnostics;
using Autofac;
using Autofac.Core;
using NDistribUnit.Common.Common.ConsoleProcessing;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.Dependencies;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Server;
using NDistribUnit.Common.Server.Communication;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Server
{
	internal class ServerProgram: GeneralProgram
	{
        /// <summary>
        /// Mains the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns></returns>
		private static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException +=
                (sender, eventArgs) =>
                new ConsoleLog().Error("Unhandled exception caught by unhandled exception handler",
                                       (Exception) eventArgs.ExceptionObject);

            try
            {
                var builder = new ContainerBuilder();
                Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var serverSettings = configuration.GetSection("settings") as ServerConfiguration;
                Debug.Assert(serverSettings != null);
                builder.RegisterType<ServerProgram>();
                builder.RegisterModule(new ServerDependenciesModule(serverSettings, args, configuration));
                IContainer container = builder.Build();
                return container.Resolve<ServerProgram>().Run();
            }
            catch (DependencyResolutionException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            catch(Exception ex)
            {
                var log = new ConsoleLog();
                log.Error("Error while running program", ex);
                Console.ReadLine();
                throw;
            }
		}

		private readonly BootstrapperParameters bootstrapperParameters;
		private readonly ILog log;
		private readonly ServerHost serverHost;

		public ServerProgram(ServerHost serverHost, BootstrapperParameters bootstrapperParameters,
		                     UpdatesMonitor updatesMonitor, ILog log)
		{
			this.bootstrapperParameters = bootstrapperParameters;
			this.updatesMonitor = updatesMonitor;
			this.log = log;
			this.serverHost = serverHost;
		}

		private int Run()
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

			var returnCode = WaitAndGetReturnCode(new[]{ bootstrapperParameters.ConfigurationFile });

			//serverHost.SaveState();
			updatesMonitor.Stop();
			serverHost.Close();

			return returnCode;
		}
	}
}