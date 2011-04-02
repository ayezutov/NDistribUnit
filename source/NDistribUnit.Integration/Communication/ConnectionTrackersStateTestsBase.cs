using NDistribUnit.Integration.Tests.General;
using NUnit.Framework;

namespace NDistribUnit.Integration.Tests.Communication
{
    public abstract class ConnectionTrackersStateTestsBase
    {
        private IntegrationTestsFixture fixture;

        [SetUp]
        public void Init()
        {
            fixture = GetTestFixture();
            fixture.Init();
        }

        protected abstract IntegrationTestsFixture GetTestFixture();

        [TearDown]
        public void TearDown()
        {
            fixture.Dispose();
        }

        [Test]
        public void ServerIsNotConnectedByDefault()
        {
            var server = fixture.StartServer();
            
            
            Assert.That(server.HasNoConnectedAgents());
        }

        [Test]
        public void AgentAndServerAreConnectedIfServerIsStartedFirst()
        {
            var server = fixture.StartServer();
            var agent = fixture.StartAgent();


            Assert.That(server.HasAConnected(agent));
        }

        [Test]
        public void AgentAndServerAreConnectedIfAgentIsStartedFirst()
        {
            var agent = fixture.StartAgent();
            var server = fixture.StartServer();

            Assert.That(server.HasAConnected(agent));
        }

        [Test]
        public void ServerDetectsAgentsExpectedShutdown()
        {
            var server = fixture.StartServer();
            var agent = fixture.StartAgent();

            Assert.That(server.HasAConnected(agent));

            agent.ShutDownInExpectedWay();

            Assert.That(server.HasADisconnected(agent));
        }

        [Test]
        public void ServerDetectsAgentsUnexpectedShutdown()
        {
            var server = fixture.StartServer();
            var agent = fixture.StartAgent();

            Assert.That(server.HasAConnected(agent));

            agent.ShutDownUngraceful();

            Assert.That(server.HasADisconnected(agent));
        }
    }
}