using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Autofac;
using NDistribUnit.Common.Agent;
using NDistribUnit.Common.Dependencies;
using NDistribUnit.Common.Server;
using NDistribUnit.Common.Server.ConnectionTracking;
using NDistribUnit.Integration.Tests.Infrastructure.Entities;

namespace NDistribUnit.Integration.Tests.Infrastructure
{
    public class NDistribUnitTestSystemController
    {
        internal const string DefaultScope = "http://dev.yezutov.com/ndistribunit/tests";
        private readonly IContainer dependenciesContainer;
        private ContainerBuilder currentBuilder;
        
        internal readonly ObservableCollection<AgentWrapper> Agents = new ObservableCollection<AgentWrapper>();
        
        private readonly ServerConfiguration serverConfiguration;
        private AgentConfiguration agentConfiguration;
        private IList<ILifetimeScope> scopes = new List<ILifetimeScope>();

        public void EnablePortsOpening()
        {
            TestingDefaultDependencies.RegisterHosts(currentBuilder);
        }

        public NDistribUnitTestSystemController()
        {
            serverConfiguration = new ServerConfiguration
                                      {
                                          PingIntervalInMiliseconds = 500,
                                          TestRunnerPort = 8091,
                                          DashboardPort = 8090,
                                          Scope = new Uri(DefaultScope)
                                      };
            agentConfiguration = new AgentConfiguration
                                     {
                                         Scope = new Uri(DefaultScope),
                                         AnnouncementInterval = TimeSpan.FromMilliseconds(1000)
                                     };

            currentBuilder = new ContainerBuilder();
            var commandLineArgs = new string[0];
            currentBuilder.RegisterInstance(this);
            currentBuilder.RegisterModule(new CommonDependenciesModule(commandLineArgs));
            currentBuilder.RegisterInstance(serverConfiguration).As<IConnectionsHostOptions>().AsSelf();
            currentBuilder.RegisterModule(new AgentDependenciesModule(agentConfiguration));
            currentBuilder.RegisterModule(new ServerDependenciesModule(serverConfiguration, commandLineArgs));
            currentBuilder.RegisterModule(new TestingDefaultDependencies(this));

            dependenciesContainer = currentBuilder.Build();
            currentBuilder = new ContainerBuilder();
        }

        public void Register<TType, TRegisterAs>() where TType : TRegisterAs
        {
            currentBuilder.RegisterType<TType>().As<TRegisterAs>();
        }

        public void Dispose()
        {
            foreach (var lifetimeScope in scopes)
            {
                lifetimeScope.Dispose();
            }
            dependenciesContainer.Dispose();
        }

        public ServerWrapper GetServer()
        {
            var server = GetContainer().Resolve<ServerWrapper>();

            return server;
        }

        private ILifetimeScope GetContainer()
        {
            FinalizeBuilder();
            ILifetimeScope scope = dependenciesContainer.BeginLifetimeScope();
            scopes.Add(scope);
            return scope;
        }

        private void FinalizeBuilder()
        {
            currentBuilder.Update(dependenciesContainer);
            currentBuilder = new ContainerBuilder();
        }

        public void UpdateNextAgentParameters(string agentName, Uri scope)
        {
            if (scope == null)
                scope = new Uri(DefaultScope);
            
            agentConfiguration = agentConfiguration.Clone();
            agentConfiguration.AgentName = agentName;
            agentConfiguration.Scope = scope;
            currentBuilder.RegisterInstance(agentConfiguration);
        }

        public AgentWrapper GetAgent()
        {
            var agent = GetContainer().Resolve<AgentWrapper>();
            Agents.Add(agent);
            return agent;
        }

        public void Register<TType>(TType entry) where TType : class
        {
            currentBuilder.RegisterInstance(entry).As<TType>().AsSelf();
        }

        public ClientWrapper GetClient()
        {
            return GetContainer().Resolve<ClientWrapper>();
        }
    }
}