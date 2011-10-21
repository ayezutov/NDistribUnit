using System;
using System.ServiceModel;
using Autofac;
using NDistribUnit.Client.Configuration;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.Dependencies;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Client
{
	/// <summary>
	/// The entry point for the client
	/// </summary>
	public class ClientProgram
	{
		private readonly ClientParameters options;
		private readonly BootstrapperParameters bootstrapperParameters;
		private readonly TestRunnerClient testRunnerClient;
		private readonly ILog log;

		static int Main(string[] args)
		{
		    var builder = new ContainerBuilder();
		    builder.Register(c => ClientParameters.Parse(args)).InstancePerLifetimeScope();
		    builder.RegisterType<ClientProgram>();
            builder.RegisterModule(new ClientDependenciesModule());
            builder.RegisterModule(new CommonDependenciesModule(args));
			var container = builder.Build();
			return container.Resolve<ClientProgram>().Run();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ClientProgram"/> class.
		/// </summary>
		/// <param name="options">Options, which were provided through command line</param>
		/// <param name="bootstrapperParameters">The bootstrapper parameters.</param>
		/// <param name="testRunnerClient">The client.</param>
		/// <param name="log">The log.</param>
		public ClientProgram(ClientParameters options, BootstrapperParameters bootstrapperParameters, TestRunnerClient testRunnerClient, ILog log)
		{
			this.options = options;
			this.bootstrapperParameters = bootstrapperParameters;
			this.testRunnerClient = testRunnerClient;
			this.log = log;
		}

		/// <summary>
		/// Runs the program with specified options
		/// </summary>
		/// <returns>A return code</returns>
		private int Run()
		{
			if (!bootstrapperParameters.AllParametersAreFilled)
			{
				log.Error("Bootstrapped application cannot be launched directly");
				return (int) ReturnCodes.CannotLaunchBootstrappedApplicationDirectly;
			}

			log.EndActivity("Client was started");
			log.BeginActivity(string.Format("Starting running test:{0}" +
											"\ttests        : '{1}'{0}" +
											"\ton           : '{2}'{0}" +
											"\tconfiguration: '{3}'{0}" +
											"\toutput file  : '{4}'",
				Environment.NewLine, string.Join(";", options.AssembliesToTest),
				options.ServerUri, options.Configuration,
				options.XmlFileName));

			if (options.ServerUri == null)
			{
				log.Error("Please provide server Uri");
				return (int) ReturnCodes.IncompleteParameterList;
			}


			try
			{
				testRunnerClient.Run();
			}
			catch(CommunicationException e)
			{
				log.Error("Server seems to be unreachable", e);
				return (int)ReturnCodes.IncompleteParameterList;
			}
			
            Console.ReadLine();
            return 0;
        }
		}
}
