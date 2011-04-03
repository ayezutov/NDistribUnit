using System;
using Autofac;
using Autofac.Core;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.Communication.ConnectionTracking;
using NDistribUnit.Common.Communication.ConnectionTracking.Announcement;
using NDistribUnit.Common.Communication.ConnectionTracking.Discovery;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Server.ConnectionTracking.Discovery;
using NDistribUnit.Common.Server.Options;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Server.Communication;
using NDistribUnit.Server.Services;

namespace NDistribUnit.Server
{
    internal class ServerProgram
    {
        private readonly ILog log;

        private static int Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ServerProgram>();
            builder.Register(c => new RollingLog(1000)).InstancePerLifetimeScope();
            builder.Register(c => new CombinedLog(new ConsoleLog(), c.Resolve<RollingLog>())).As<ILog>().InstancePerLifetimeScope();
            builder.Register(c => ServerParameters.Parse(args)).InstancePerLifetimeScope();
            builder.RegisterType<TestRunnerServer>().InstancePerLifetimeScope();
            builder.RegisterType<DashboardService>().InstancePerLifetimeScope();

#pragma warning disable 162
            if (true)
            {
                builder.Register(c => new DiscoveryConnectionsTracker<ITestRunnerAgent>(new DiscoveryOptions()
                                                                                            {
                                                                                                Scope =
                                                                                                    new Uri(
                                                                                                    "http://hubwoo.com/trr-odc"),
                                                                                                DiscoveryIntervalInMiliseconds
                                                                                                    = 20000,
                                                                                                PingIntervalInMiliseconds
                                                                                                    = 5000
                                                                                            }, c.Resolve<ILog>()))
                    .As<IConnectionsTracker<ITestRunnerAgent>>();
            }
            else

            {
                builder.Register(c => new AnnouncementConnectionsTracker<ITestRunnerAgent>(new AnnouncementConnectionsTrackerOptions()
                {
                    Scope =
                        new Uri(
                        "http://hubwoo.com/trr-odc"),
                    PingIntervalInMiliseconds
                        = 5000
                }, c.Resolve<ILog>()))
                    .As<IConnectionsTracker<ITestRunnerAgent>>();
            }
#pragma warning restore 162
            builder.Register(c => new ServerConnectionsTracker(c.Resolve<IConnectionsTracker<ITestRunnerAgent>>(), c.Resolve<ILog>()))
                .InstancePerLifetimeScope();
            builder.Register(
                c =>
                new ServerHost(c.Resolve<ServerParameters>().DashboardPort, 
                    c.Resolve<ServerParameters>().TestRunnerPort,
                    c.Resolve<TestRunnerServer>(),
                    c.Resolve<DashboardService>(),
                    c.Resolve<ServerConnectionsTracker>(),
                    c.Resolve<ILog>()
                    )).InstancePerLifetimeScope();

            var container = builder.Build();
            return container.Resolve<ServerProgram>().Run();
        }

        private ServerParameters Options { get; set; }
        private ServerHost ServerHost { get; set; }

        public ServerProgram(ServerParameters options, ServerHost serverHost, ILog log)
        {
            this.log = log;
            Options = options;
            ServerHost = serverHost;
        }

        private int Run()
        {
            log.BeginActivity("Server is starting...");
            ServerHost.Start();
            log.EndActivity("Server was started. Please press <Enter> to exit");
            Console.ReadLine();
            return 0;
        }
    }
}