using System;
using System.Reflection;
using System.ServiceModel;
using Autofac;
using NDistribUnit.Client.Configuration;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Common.Updating;
using NDistribUnit.Common.Updating.Updaters;

namespace NDistribUnit.Client
{
    /// <summary>
    /// The entry point for the client
    /// </summary>
    public class ClientProgram
    {
    	private readonly UpdateReceiver updateReceiver;
    	private readonly ClientUpdater updater;
    	private readonly ILog log;

        static int Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ClientProgram>();
            builder.RegisterType<ClientUpdater>();
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
		/// <param name="updater">The updater.</param>
		/// <param name="log">The log.</param>
        public ClientProgram(ClientParameters options, UpdateReceiver updateReceiver, ClientUpdater updater, ILog log)
		{
			this.updateReceiver = updateReceiver;
			this.updater = updater;
			this.log = log;
		}

    	/// <summary>
        /// Runs the program with specified options
        /// </summary>
        /// <returns>A return code</returns>
    	private int Run()
        {
			try
			{
				var channel = ChannelFactory<ITestRunnerServer>
					.CreateChannel(new NetTcpBinding("NDistribUnit.Default"), 
						new EndpointAddress(string.Format("net.tcp://{0}:{1}", Environment.MachineName, 8009)));

				log.Info("Checking for updates...");
				var updateResult = channel.GetUpdatePackage(Assembly.GetEntryAssembly().GetName().Version);
				if (updateReceiver.SaveUpdatePackage(updateResult))
					updater.PerformUpdate();
			}
			catch(CommunicationException ex)
			{
				log.Error("Error while trying to get the update", ex);
			}

        	log.EndActivity("Client was started");
            Console.ReadLine();
            return 0;
        }
    }
}
