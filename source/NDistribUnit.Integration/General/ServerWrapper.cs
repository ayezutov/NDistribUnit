using System;
using System.Runtime.Serialization.Formatters;
using Autofac;
using NDistribUnit.Common.Logging;
using NDistribUnit.Server;
using NDistribUnit.Server.Communication;
using NDistribUnit.Server.Services;

namespace NDistribUnit.Integration.Tests.General
{
    /// <summary>
    /// The class is a special wrapper around real server program to allow easy access
    /// in testing code
    /// </summary>
    public class ServerWrapper: IDisposable
    {
        private readonly ServerHost serverHost;

        private ServerWrapper(ServerHost serverHost)
        {
            this.serverHost = serverHost;
        }

        public static ServerWrapper Any { get; private set; }

        public static ServerWrapper Start()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new ServerHost(9098, 9099, 
                c.Resolve<TestRunnerServer>(), 
                c.Resolve<DashboardService>(), 
                c.Resolve<ServerConnectionsTracker>(),
                new ConsoleLog()));
            builder.Register(c => new TestRunnerServer());
            builder.Register(c => new DashboardService(c.Resolve<ServerConnectionsTracker>(), new RollingLog(5)));
            builder.Register(c => new ServerConnectionsTracker("http://hubwoo.com", new ConsoleLog()));
            var container = builder.Build();
            var host = container.Resolve<ServerHost>();
            var serverWrapper = new ServerWrapper(host);
            host.Start();
            return serverWrapper;
        }

        public bool IsConnectedTo(AgentWrapper agent)
        {
            throw new NotImplementedException();
        }

        public bool IsNotConnectedTo(AgentWrapper agent)
        {
            return !IsConnectedTo(agent);
        }

        public void ShutDownInExpectedWay()
        {
            serverHost.Close();
        }

        public void ShutDownUngraceful()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            ShutDownInExpectedWay();
        }
    }
}