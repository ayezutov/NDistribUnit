using System;
using System.Collections.Generic;
using Autofac;
using NDistribUnit.Common.Agent;
using NDistribUnit.Common.Agent.ExternalModules;
using NDistribUnit.Common.Communication.ConnectionTracking;
using NDistribUnit.Common.Communication.ConnectionTracking.Announcement;
using NDistribUnit.Common.Communication.ConnectionTracking.Discovery;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Server.Communication;
using NDistribUnit.Common.Server.ConnectionTracking.Discovery;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Server.Communication;
using NDistribUnit.Server.Services;

namespace NDistribUnit.Integration.Tests.General
{
    public class IntegrationTestsFixture
    {
        private IContainer container;
        private readonly IList<ServerWrapper> servers = new List<ServerWrapper>();
        private readonly IList<AgentWrapper> agents = new List<AgentWrapper>();
        private Type connectionsTrackerType = typeof(DiscoveryConnectionsTracker<>);

        public void Init()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ConsoleLog>().As<ILog>();
            builder.Register(c => new ServerHost(9098, 9099,
                c.Resolve<TestRunnerServer>(),
                c.Resolve<DashboardService>(),
                c.Resolve<ServerConnectionsTracker>(),
                c.Resolve<ILog>()));
            builder.Register(c => new TestRunnerServer());
            builder.Register(c => new DashboardService(c.Resolve<ServerConnectionsTracker>(), new RollingLog(5)));

            if (connectionsTrackerType == typeof(DiscoveryConnectionsTracker<>))
            {
                builder.Register(
                    c => new DiscoveryConnectionsTracker<ITestRunnerAgent>(new DiscoveryOptions()
                                                                               {
                                                                                   Scope =
                                                                                       new Uri(
                                                                                       "http://ndistribunit.com/tests"),
                                                                                   DiscoveryIntervalInMiliseconds = 1000,
                                                                                   PingIntervalInMiliseconds = 500
                                                                               }, c.Resolve<ILog>()))
                    .As<IConnectionsTracker<ITestRunnerAgent>>();
            }
            else if (connectionsTrackerType == typeof(AnnouncementConnectionsTracker<>))
            {
                builder.Register(
                    c => new AnnouncementConnectionsTracker<ITestRunnerAgent>(new AnnouncementConnectionsTrackerOptions()
                    {
                        Scope =
                            new Uri(
                            "http://ndistribunit.com/tests"),
                        PingIntervalInMiliseconds = 500
                    }, c.Resolve<ILog>()))
                    .As<IConnectionsTracker<ITestRunnerAgent>>();
            }

            builder.Register(c => new ServerConnectionsTracker(c.Resolve<IConnectionsTracker<ITestRunnerAgent>>(), c.Resolve<ILog>()));
            builder.Register(c => new TestRunnerAgentService(c.Resolve<ILog>(), "Agent #1", null));
            builder.Register(c => new AgentHost(c.Resolve<TestRunnerAgentService>(), new IAgentExternalModule[]
                                                    {
                                                        new DiscoveryModule(new Uri("http://ndistribunit.com/tests")),
                                                        new AnnouncementModule(TimeSpan.FromMilliseconds(200), new Uri("http://ndistribunit.com/tests"), c.Resolve<ILog>()), 
                                                    }, c.Resolve<ILog>()));
            container = builder.Build();
        }

        public ServerWrapper StartServer()
        {
            var server = new ServerWrapper(container.Resolve<ServerHost>());
            servers.Add(server);
            server.Start();
            return server;
        }

        public AgentWrapper StartAgent(string agentName = null)
        {
            if (agentName != null)
            {
                var builder = new ContainerBuilder();
                builder.Register(c => new TestRunnerAgentService(c.Resolve<ILog>(), agentName, null));
                builder.Update(container);
            }
            var agent = new AgentWrapper(container.Resolve<AgentHost>());
            agents.Add(agent);
            agent.Start();
            return agent;
        }

        public void Dispose()
        {
            foreach (var server in servers)
            {
                server.Dispose();
            }

            foreach (var agent in agents)
            {
                agent.Dispose();
            }
        }

        public void SetConnectionsTracker(Type type)
        {
            this.connectionsTrackerType = type;
        }
    }
}