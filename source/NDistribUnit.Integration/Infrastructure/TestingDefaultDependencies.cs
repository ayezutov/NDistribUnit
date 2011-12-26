using System;
using System.Reflection;
using Autofac;
using NDistribUnit.Common.Agent;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Common.Networking;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Server.AgentsTracking.AgentsProviders;
using NDistribUnit.Common.Server.Communication;
using NDistribUnit.Common.Updating;
using NDistribUnit.Integration.Tests.Infrastructure.Entities;
using NDistribUnit.Integration.Tests.Infrastructure.Stubs;
using Moq;
using Module = Autofac.Module;

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

            var repo = new MockRepository(MockBehavior.Default);

            builder.RegisterType<ClientWrapper>();
            builder.RegisterType<TestingAgentsProvider>().As<IAgentsProvider>();

            builder.RegisterType<TestUpdateReceiver>().AsSelf().As<IUpdateReceiver>().InstancePerLifetimeScope();
            builder.Register(c => new TestingVersionProvider(new Version(1, 0, 0, 0)))
                .As<IVersionProvider>()
                .AsSelf()
                .InstancePerLifetimeScope();

            var updateSource = repo.Create<IUpdateSource>();
            updateSource
                .Setup(s => s.GetZippedVersionFolder(It.IsAny<Version>()))
                .Returns(new[] {(byte) 'c'});

            builder.RegisterInstance(updateSource.Object).As<IUpdateSource>();

            builder.RegisterType<TestingConnectionProvider>().As<IConnectionProvider>().SingleInstance();
            builder.Register(c => new BootstrapperParameters
                                      {
                                          BootstrapperFile = Assembly.GetExecutingAssembly().Location
                                      });
        }

        internal static void RegisterHosts(ContainerBuilder builder)
        {
            builder.RegisterType<ServerHost>();
            builder.RegisterType<AgentHost>();

            builder.RegisterType<ServerWrapper>();
            builder.RegisterType<AgentWrapper>();
            builder.RegisterType<RealConnectionProvider>().As<IConnectionProvider>().SingleInstance();
        }
    }
}