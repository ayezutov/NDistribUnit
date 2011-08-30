using System;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;
using Autofac;
using NDistribUnit.Client.Configuration;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Client
{
	/// <summary>
	/// The entry point for the client
	/// </summary>
	public class ClientProgram
	{
		private readonly ClientParameters options;
		private readonly UpdateReceiver updateReceiver;
		private readonly BootstrapperParameters bootstrapperParameters;
		private readonly ILog log;

		static int Main(string[] args)
		{
			var builder = new ContainerBuilder();
			builder.RegisterType<ClientProgram>();
			builder.RegisterType<UpdateReceiver>();
			builder.RegisterType<VersionDirectoryFinder>();
			builder.Register(c => BootstrapperParameters.Parse(args)).InstancePerLifetimeScope();
			builder.RegisterType<ConsoleLog>().As<ILog>();
			builder.Register(c => ClientParameters.Parse(args)).InstancePerLifetimeScope();

			var container = builder.Build();

			return container.Resolve<ClientProgram>().Run();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ClientProgram"/> class.
		/// </summary>
		/// <param name="options">Options, which were provided through command line</param>
		/// <param name="updateReceiver">The update receiver.</param>
		/// <param name="bootstrapperParameters">The bootstrapper parameters.</param>
		/// <param name="log">The log.</param>
		public ClientProgram(ClientParameters options, UpdateReceiver updateReceiver, BootstrapperParameters bootstrapperParameters, ILog log)
		{
			this.options = options;
			this.updateReceiver = updateReceiver;
			this.bootstrapperParameters = bootstrapperParameters;
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

			var waitingForTestsToCompleteTask = new Task(() =>
			         	{

			         		
			         		log.BeginActivity("Doing some work");
			         		log.EndActivity("Finished doing work");
			         	});
			
			var updateTask = new Task(()=>
				{
					try
						{
							log.Info("Checking for updates...");
							
							var channel = ChannelFactory<ITestRunnerServer>
									.CreateChannel(new NetTcpBinding("NDistribUnit.Default"),
										new EndpointAddress(string.Format("net.tcp://{0}:{1}", Environment.MachineName, 8009)));

							
							var updateResult = channel.GetUpdatePackage(Assembly.GetEntryAssembly().GetName().Version);
							log.Info(updateReceiver.SaveUpdatePackage(updateResult)
							         	? string.Format("Update was received: {0}", updateResult.Version)
							         	: "No updates available");
						}
						catch (CommunicationException ex)
						{
							log.Error("Error while trying to get the update", ex);
						}
				});

			waitingForTestsToCompleteTask.Start();
			updateTask.Start();

			Task.WaitAll(waitingForTestsToCompleteTask, updateTask);
			
            Console.ReadLine();
            return 0;
        }
	}
}
