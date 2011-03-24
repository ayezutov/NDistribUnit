using System.Threading;
using NDistribUnit.Integration.Tests.General;
using NUnit.Framework;

namespace NDistribUnit.Integration.Tests
{
    [TestFixture, Ignore("The implementation of these tests was started, when the current approach for server/agent was not yet chosen. It is required to update them.")]
    public class ConnectionTrackersStateTests: IntegrationTestBase
    {
        private ServerWrapper Server { get; set; }
        private AgentWrapper Agent { get; set; }

        [TearDown]
        public void TearDown()
        {
            if (Server != null)
                Server.Dispose();

            if (Agent != null)
                Agent.Dispose();
        }

        [Test]
        public void AgentIsNotConnectedByDefault()
        {
            Agent = AgentWrapper.Start();

            Assert.That(Agent.IsNotConnectedTo(ServerWrapper.Any));
        }


        [Test]
        public void ServerIsNotConnectedByDefault()
        {
            Server = ServerWrapper.Start();
            
            Assert.That(Server.IsNotConnectedTo(AgentWrapper.Any));
        }

        [Test]
        public void AgentAndServerAreConnectedIfServerIsStartedFirst()
        {
            Server = ServerWrapper.Start();
            Agent = AgentWrapper.Start();

            Assert.That(Agent.IsConnectedTo(Server));
            Assert.That(Server.IsConnectedTo(Agent));

            Server.Dispose();
        }

        [Test]
        public void AgentAndServerAreConnectedIfAgentIsStartedFirst()
        {
            Agent = AgentWrapper.Start();
            Server = ServerWrapper.Start();

            Thread.Sleep(2000);

            Assert.That(Agent.IsConnectedTo(Server));
            Assert.That(Server.IsConnectedTo(Agent));
        }

        [Test]
        public void AgentDetectsServersExpectedShutdown()
        {
            Server = ServerWrapper.Start();
            Agent = AgentWrapper.Start();

            Assert.That(Agent.IsConnectedTo(Server));
            Assert.That(Server.IsConnectedTo(Agent));

            Server.ShutDownInExpectedWay();

            Assert.That(Agent.IsNotConnectedTo(Server));
        }

        [Test]
        public void AgentDetectsServersUnexpectedShutdown()
        {
            Server = ServerWrapper.Start();
            Agent = AgentWrapper.Start();

            Assert.That(Agent.IsConnectedTo(Server));
            Assert.That(Server.IsConnectedTo(Agent));

            Server.ShutDownUngraceful();

            Assert.That(Agent.IsNotConnectedTo(Server));
        }

        [Test]
        public void ServerDetectsAgentsExpectedShutdown()
        {
            Server = ServerWrapper.Start();
            Agent = AgentWrapper.Start();

            Assert.That(Agent.IsConnectedTo(Server));
            Assert.That(Server.IsConnectedTo(Agent));

            Agent.ShutDownInExpectedWay();

            Assert.That(Server.IsNotConnectedTo(Agent));
        }

        [Test]
        public void ServerDetectsAgentsUnexpectedShutdown()
        {
            Server = ServerWrapper.Start();
            Agent = AgentWrapper.Start();

            Assert.That(Agent.IsConnectedTo(Server));
            Assert.That(Server.IsConnectedTo(Agent));

            Agent.ShutDownUngraceful();

            Assert.That(Server.IsNotConnectedTo(Agent));
        }


    }

    public class IntegrationTestBase
    {
    }
}
