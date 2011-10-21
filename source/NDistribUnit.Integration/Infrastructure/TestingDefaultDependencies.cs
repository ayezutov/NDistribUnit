using System;
using System.ServiceModel;
using Autofac;
using NDistribUnit.Common.Agent;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Common.Networking;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Communication.ConnectionTracking;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.Server.Communication;
using NDistribUnit.Common.Updating;
using NDistribUnit.Integration.Tests.Infrastructure.Entities;
using NDistribUnit.Integration.Tests.Infrastructure.Stubs;
using Moq;
using System.Linq;

namespace NDistribUnit.Integration.Tests.Infrastructure
{
    internal class TestingDefaultDependencies : Module
    {
        private readonly NDistribUnitTestSystemController controller;

        public TestingDefaultDependencies(NDistribUnitTestSystemController controller)
        {
            this.controller = controller;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ServerWrapper>().WithParameter(
                (a,c) => a.ParameterType == typeof (ServerHost), 
                (b,c) => null).OwnedByLifetimeScope();
            builder.RegisterType<AgentWrapper>().WithParameter(
                (a, c) => a.ParameterType == typeof(AgentHost),
                (b, c) => null).OwnedByLifetimeScope();

            var repo = new MockRepository(MockBehavior.Default);

            builder.RegisterType<ClientWrapper>();
            builder.RegisterType<TestingConnectionTracker>().As<INetworkExplorer<ITestRunnerAgent>>();

            var versionProvider = repo.Create<IVersionProvider>();
            versionProvider.Setup(p => p.GetVersion()).Returns(new Version(1, 0, 0, 0));

            builder.RegisterType<TestUpdateReceiver>().AsSelf().As<IUpdateReceiver>().InstancePerLifetimeScope();
            builder.Register(c => versionProvider.Object).As<IVersionProvider>();

            var updateSource = repo.Create<IUpdateSource>();
            updateSource
                .Setup(s => s.GetZippedVersionFolder(It.IsAny<Version>()))
                .Returns(new[] {(byte) 'c'});

            builder.RegisterInstance(updateSource.Object).As<IUpdateSource>();

            var connectionProviderMock = repo.Create<IConnectionProvider>();
            connectionProviderMock
                .Setup(cp => cp.GetConnection<ITestRunnerAgent>(It.IsAny<EndpointAddress>()))
                .Returns((EndpointAddress address) =>
                             {
                                 AgentWrapper agentWrapper = controller.Agents
                                     .FirstOrDefault(a => a.TestRunner.Name.Equals(address.Uri.AbsolutePath.Trim('/')));

                                 return agentWrapper == null ? null : agentWrapper.TestRunner;
                             });
            builder.Register(c => connectionProviderMock.Object).As<IConnectionProvider>().InstancePerLifetimeScope();
        }

        internal static void RegisterHosts(ContainerBuilder builder)
        {
            builder.RegisterType<ServerHost>();
            builder.RegisterType<AgentHost>();

            builder.RegisterType<ServerWrapper>();
            builder.RegisterType<AgentWrapper>();
            builder.RegisterType<RealConnectionProvider>().As<IConnectionProvider>().InstancePerLifetimeScope();
        }
    }
}