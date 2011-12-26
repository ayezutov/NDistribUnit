using System;
using System.Collections.Generic;
using Autofac;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Server.AgentsTracking.AgentsProviders;
using NDistribUnit.Integration.Tests.Infrastructure.Entities;
using NDistribUnit.Integration.Tests.Infrastructure.Stubs;

namespace NDistribUnit.Integration.Tests.Infrastructure
{
    public class NDistribUnitTestSystemFluent
    {
        internal NDistribUnitTestSystem System { get; private set; }

        public NDistribUnitTestSystemFluent()
        {
            System = new NDistribUnitTestSystem();
        }

        public ServerWrapper StartServer()
        {
            var server = System.GetServer();
            server.Start();
            return server;
        }

        public AgentWrapper StartAgent(string agentName = null, Uri agentScope = null)
        {
            UpdateNextAgentParameters(agentName ?? Guid.NewGuid().ToString(), agentScope);
            var agent = System.GetAgent();
            agent.Start();
            return agent;
        }

        public void UpdateNextAgentParameters(string agentName, Uri scope)
        {
            if (scope == null)
                scope = new Uri(NDistribUnitTestSystem.DefaultScope);

            System.AgentConfiguration = System.AgentConfiguration.Clone();
            System.AgentConfiguration.AgentName = agentName;
            System.AgentConfiguration.Scope = scope;
            System.CurrentBuilder.RegisterInstance(System.AgentConfiguration);
        }

        public ClientWrapper GetClient()
        {
            return System.GetClient();
        }

        public void Dispose()
        {
            System.Dispose();
        }

        #region Configuration methods

        public NDistribUnitTestSystemFluent SetConnectionsTracker<TTracker>()
            where TTracker : IAgentsProvider
        {
            System.CurrentBuilder.RegisterType<TTracker>();
            System.CurrentBuilder.Register<IEnumerable<IAgentsProvider>>(c => new IAgentsProvider[]
                                                                           {
                                                                               c.Resolve<TTracker>()
                                                                           });
            return this;
        }

        public NDistribUnitTestSystemFluent ActAsRealSystemWithOpeningPorts()
        {
            TestingDefaultDependencies.RegisterHosts(System.CurrentBuilder);
            return this;
        }

        public NDistribUnitTestSystemFluent Register<TType>(TType entry) where TType : class
        {
            System.CurrentBuilder.Register(c => entry)
                .As<TType>()
                .AsSelf()
                .InstancePerLifetimeScope();
            return this;
        }

        public NDistribUnitTestSystemFluent OfVersion(Version version)
        {
            System.CurrentBuilder.Register(c => new TestingVersionProvider(version))
                .As<IVersionProvider>()
                .AsSelf()
                .InstancePerLifetimeScope();

            return this;
        }

        #endregion
    }
}