using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using Autofac;
using Autofac.Core;
using NDistribUnit.Common.Common.ConsoleProcessing;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.ConsoleProcessing;
using NDistribUnit.Common.Dependencies;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Server;
using NDistribUnit.Common.Server.Communication;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Server
{
	internal class ServerProgram: GeneralProgram
	{
		private static int Main(string[] args)
		{
            try
            {
                var builder = new ContainerBuilder();
                Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                builder.RegisterInstance(configuration);

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

			var returnCode = WaitAndGetReturnCode();

			//serverHost.SaveState();
			updatesMonitor.Stop();
			serverHost.Close();

			return returnCode;
		}
	}
}