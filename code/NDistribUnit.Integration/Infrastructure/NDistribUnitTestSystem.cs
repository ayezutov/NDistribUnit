using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Autofac;
using NDistribUnit.Common.Agent;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Dependencies;
using NDistribUnit.Common.Server;
using NDistribUnit.Common.Server.AgentsTracking;
using NDistribUnit.Integration.Tests.Infrastructure.Entities;

namespace NDistribUnit.Integration.Tests.Infrastructure
{
    public class NDistribUnitTestSystem
    {
        internal const string DefaultScope = "http://dev.yezutov.com/ndistribunit/tests";
        private readonly IContainer dependenciesContainer;
        internal ContainerBuilder CurrentBuilder { get; private set; }
        
        internal readonly ObservableCollection<AgentWrapper> Agents = new ObservableCollection<AgentWrapper>();
        
        private readonly ServerConfiguration serverConfiguration;
        internal AgentConfiguration AgentConfiguration { get; set; }
        private readonly IList<ILifetimeScope> scopes = new List<ILifetimeScope>();
        private readonly ClientParameters clientParameters;

        public ServerWrapper Server { get; private set; }

        public NDistribUnitTestSystem()
        {
            serverConfiguration = new ServerConfiguration
                                      {
                                          PingIntervalInMiliseconds = 500,
                                          TestRunnerPort = 8091,
                                          DashboardPort = 8090,
                                          Scope = new Uri(DefaultScope)
                                      };
            AgentConfiguration = new AgentConfiguration
                                     {
                                         Scope = new Uri(DefaultScope),
                                         AnnouncementInterval = TimeSpan.FromMilliseconds(1000)
                                     };
            clientParameters = new ClientParameters
                                   {
                                       ServerUri = new Uri("test://server"),
                                       TimeoutPeriod = TimeSpan.FromSeconds(1),
                                       NUnitParameters = {Configuration = "Debug"}
                                   };
            clientParameters.NUnitParameters.AssembliesToTest.Add("test://ndistribunit.org/project.nunit");

            CurrentBuilder = new ContainerBuilder();
            var commandLineArgs = new string[0];
            CurrentBuilder.RegisterInstance(this);
            CurrentBuilder.RegisterModule(new CommonDependenciesModule());
            CurrentBuilder.RegisterInstance(serverConfiguration).As<IConnectionsHostOptions>().AsSelf();
            CurrentBuilder.RegisterInstance(clientParameters);
            CurrentBuilder.RegisterModule(new AgentDependenciesModule(AgentConfiguration, commandLineArgs));
            CurrentBuilder.RegisterModule(new ServerDependenciesModule(serverConfiguration));
            CurrentBuilder.RegisterModule(new ClientDependenciesModule());
            CurrentBuilder.RegisterModule(new TestingDefaultDependencies());

            dependenciesContainer = CurrentBuilder.Build();
            CurrentBuilder = new ContainerBuilder();
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
            return Server = GetContainer().Resolve<ServerWrapper>();
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
            CurrentBuilder.Update(dependenciesContainer);
            CurrentBuilder = new ContainerBuilder();
        }
        
        public AgentWrapper GetAgent()
        {
            var agent = GetContainer().Resolve<AgentWrapper>();
            Agents.Add(agent);
            return agent;
        }

        public void Register<TType>(TType entry) where TType : class
        {
            CurrentBuilder.RegisterInstance(entry).As<TType>().AsSelf();
        }

        public ClientWrapper GetClient()
        {
            return GetContainer().Resolve<ClientWrapper>();
        }
    }
}