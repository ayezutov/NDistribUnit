using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autofac;
using NDistribUnit.Common.Communication.ConnectionTracking;
using NDistribUnit.Common.Communication.ConnectionTracking.Announcement;
using NDistribUnit.Common.Communication.ConnectionTracking.Discovery;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Server.Communication;
using NDistribUnit.Common.Server.ConnectionTracking.Discovery;
using NDistribUnit.Common.Server.Options;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Common.Updating;
using NDistribUnit.Server.Communication;
using NDistribUnit.Server.Services;

namespace NDistribUnit.Server
{
	internal class ServerProgram
	{
		private static int Main(string[] args)
		{
			var builder = new ContainerBuilder();
			RegisterDependencies(args, builder);
			var container = builder.Build();
			return container.Resolve<ServerProgram>().Run();
		}

		private readonly BootstrapperParameters bootstrapperParameters;
		private readonly UpdatesAvailabilityMonitor updatesMonitor;
		private readonly ILog log;
		private readonly ServerHost serverHost;

		public ServerProgram(ServerHost serverHost, BootstrapperParameters bootstrapperParameters,
		                     UpdatesAvailabilityMonitor updatesMonitor, ILog log)
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
				log.Error("This programm cannot be launched directly");
				return 2;
			}

			if (bootstrapperParameters.IsDebug && !Debugger.IsAttached)
				Debugger.Launch();

			updatesMonitor.Start();

			log.BeginActivity("Server is starting...");
			serverHost.Start();
			log.EndActivity("Server was started. Please press <Enter> to exit");
			Console.ReadLine();
			return 0;
		}

		private static void RegisterDependencies(IEnumerable<string> args, ContainerBuilder builder)
		{
			builder.RegisterType<ServerProgram>();
			builder.Register(c => new RollingLog(1000)).InstancePerLifetimeScope();
			builder.Register(c => new CombinedLog(new ConsoleLog(), c.Resolve<RollingLog>()))
				.As<ILog>().InstancePerLifetimeScope();
			builder.Register(c => ServerParameters.Parse(args)).InstancePerLifetimeScope();
			builder.Register(c => BootstrapperParameters.Parse(args)).InstancePerLifetimeScope();
			builder.RegisterType<TestRunnerServer>().InstancePerLifetimeScope();
			builder.RegisterType<DashboardService>().InstancePerLifetimeScope();
			builder.RegisterType<ServerConnectionsTracker>().InstancePerLifetimeScope();
			builder.RegisterType<UpdatesAvailabilityMonitor>();
			builder.RegisterType<UpdateSource>().As<IUpdateSource>();
			builder.RegisterType<ServerUpdater>().As<IUpdater>();
			builder.Register(
				c =>
				new ServerHost(c.Resolve<ServerParameters>().DashboardPort,
				               c.Resolve<ServerParameters>().TestRunnerPort,
				               c.Resolve<TestRunnerServer>(),
				               c.Resolve<DashboardService>(),
				               c.Resolve<ServerConnectionsTracker>(),
				               c.Resolve<ILog>()
					)).InstancePerLifetimeScope();
#pragma warning disable 162
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable RedundantIfElseBlock
			if (false)
			{
				builder.Register(c => new DiscoveryConnectionsTracker<ITestRunnerAgent>(
				                      	new DiscoveryOptions
				                      		{
				                      			Scope = new Uri("http://hubwoo.com/trr-odc"),
				                      			DiscoveryIntervalInMiliseconds = 20000,
				                      			PingIntervalInMiliseconds = 5000
				                      		}, c.Resolve<ILog>()))
					.As<IConnectionsTracker<ITestRunnerAgent>>();
			}
			else
			{
				builder.Register(c => new AnnouncementConnectionsTracker<ITestRunnerAgent>(
				                      	new AnnouncementConnectionsTrackerOptions
				                      		{
				                      			Scope = new Uri("http://hubwoo.com/trr-odc"),
				                      			PingIntervalInMiliseconds = 5000
				                      		}, c.Resolve<ILog>()))
					.As<IConnectionsTracker<ITestRunnerAgent>>();
			}
// ReSharper restore RedundantIfElseBlock
// ReSharper restore HeuristicUnreachableCode
#pragma warning restore 162
		}
	}
}