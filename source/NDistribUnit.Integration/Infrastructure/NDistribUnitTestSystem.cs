using System;
using Moq;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Communication.ConnectionTracking;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Integration.Tests.Infrastructure.Entities;

namespace NDistribUnit.Integration.Tests.Infrastructure
{
    public class NDistribUnitTestSystem
    {
        private readonly NDistribUnitTestSystemController controller;

        public NDistribUnitTestSystem()
        {
            controller = new NDistribUnitTestSystemController();
            
        }

        public ServerWrapper StartServer()
        {
            var server = controller.GetServer();
            server.Start();
            return server;
        }

        public AgentWrapper StartAgent(string agentName = null, Uri agentScope = null)
        {
            controller.UpdateNextAgentParameters(agentName ?? Guid.NewGuid().ToString(), agentScope);
            var agent = controller.GetAgent();
            agent.Start();
            return agent;
        }

        public ClientWrapper GetClient()
        {
            return controller.GetClient();
        }

        public void Dispose()
        {
            controller.Dispose();
        }

        #region Configuration methods

        public NDistribUnitTestSystem SetConnectionsTracker<TTracker>()
            where TTracker : INetworkExplorer<IRemoteAppPart>
        {
            controller.Register<TTracker, INetworkExplorer<IRemoteAppPart>>();
            return this;
        }

        public NDistribUnitTestSystem ActAsRealSystemWithOpeningPorts()
        {
            controller.EnablePortsOpening();
            return this;
        }

        public NDistribUnitTestSystem Register<TType>(TType entry) where TType : class
        {
            controller.Register(entry);
            return this;
        }

        public NDistribUnitTestSystem OfVersion(Version version)
        {
            var versionProvider = new Mock<IVersionProvider>();
            versionProvider.Setup(p => p.GetVersion()).Returns(version);
            controller.Register(versionProvider.Object);
            return this;
        }

        #endregion
    }
}