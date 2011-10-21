using System;
using Autofac;
using NDistribUnit.Common.Agent;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Communication.ConnectionTracking;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.Server.Communication;
using NDistribUnit.Common.Updating;
using NDistribUnit.Integration.Tests.Infrastructure.Entities;
using NDistribUnit.Integration.Tests.Infrastructure.Stubs;
using Moq;

namespace NDistribUnit.Integration.Tests.Infrastructure
{
    internal class TestingDefaultDependencies : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ServerWrapper>().WithParameter(
                (a,c) => a.ParameterType == typeof (ServerHost), 
                (b,c) => null).OwnedByLifetimeScope();
            builder.RegisterType<AgentWrapper>().WithParameter(
                (a, c) => a.ParameterType == typeof(AgentHost),
                (b, c) => null).OwnedByLifetimeScope();

            builder.RegisterType<ClientWrapper>();
            builder.RegisterType<TestingConnectionTracker>().As<IConnectionsTracker<ITestRunnerAgent>>();

            var versionProvider = new Mock<IVersionProvider>();
            versionProvider.Setup(p => p.GetVersion()).Returns(new Version(1, 0, 0, 0));

            builder.RegisterType<TestUpdateReceiver>().AsSelf().As<IUpdateReceiver>();
            builder.Register(c => versionProvider.Object).As<IVersionProvider>();

            var updateSource = new Mock<IUpdateSource>();
            updateSource
                .Setup(s => s.GetZippedVersionFolder(It.IsAny<Version>()))
                .Returns(new[] {(byte) 'c'});

            builder.RegisterInstance(updateSource.Object).As<IUpdateSource>();
        }

        internal static void RegisterHosts(ContainerBuilder builder)
        {
            builder.RegisterType<ServerHost>();
            builder.RegisterType<AgentHost>();

            builder.RegisterType<ServerWrapper>();
            builder.RegisterType<AgentWrapper>();
        }
    }
}