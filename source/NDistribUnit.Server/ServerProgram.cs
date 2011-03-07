using System;
using Autofac;
using Autofac.Core;
using NDistribUnit.Server.Communication;
using NDistribUnit.Server.Services;

namespace NDistribUnit.Server
{
    internal class ServerProgram
    {
        private static int Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ServerProgram>();
            builder.Register(c => ServerParameters.Parse(args)).InstancePerLifetimeScope();
            builder.RegisterType<TestRunnerServer>().InstancePerLifetimeScope();
            builder.RegisterType<DashboardService>().InstancePerLifetimeScope();
            builder.Register(c => new ServerConnectionsTracker("http://hubwoo.com/trr-odc")).InstancePerLifetimeScope();
            builder.Register(
                c =>
                new ServerHost(c.Resolve<ServerParameters>().DashboardPort, 
                    c.Resolve<ServerParameters>().DashboardPort,
                    c.Resolve<TestRunnerServer>(),
                    c.Resolve<DashboardService>(),
                    c.Resolve<ServerConnectionsTracker>())).InstancePerLifetimeScope();
            var container = builder.Build();
            return container.Resolve<ServerProgram>().Run();
        }

        private ServerParameters Options { get; set; }
        private ServerHost ServerHost { get; set; }

        public ServerProgram(ServerParameters options, ServerHost serverHost)
        {
            Options = options;
            ServerHost = serverHost;
        }

        private int Run()
        {
            Console.WriteLine("Server is starting...");
            ServerHost.Start();
            Console.WriteLine("Server was started. Please press <Enter> to exit");
            Console.ReadLine();
            return 0;
        }
    }
}