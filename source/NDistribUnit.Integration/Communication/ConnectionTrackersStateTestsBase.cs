using System.Threading;
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

        /// <summary>
        /// This can happen if between pings the agent is closed 
        /// and two new instances are opened and one on the same port.
        /// 
        /// Pure syntetic case. May happen very seldom
        /// </summary>
        [Test]
        public void AgentNameChangeIsCorrectlyHandledByServer()
        {
            var server = fixture.StartServer();
            var agent = fixture.StartAgent("Agent #1 Name 1");

            Assert.That(server.HasAConnected("Agent #1 Name 1"));

            agent.ChangeNameTo("Agent #1 Name 2");

            Assert.That(server.HasADisconnected("Agent #1 Name 1"));
            Assert.That(server.HasAConnected("Agent #1 Name 2"));
        }
        
        /// <summary>
        /// This can happen if between pings the agent is restarted. 
        /// The name is same, but the port changes
        /// </summary>
        [Test]
        public void AgentPortChangeIsCorrectlyHandledByServer()
        {
            var server = fixture.StartServer();
            var agent = fixture.StartAgent("Agent #1 Name 1");
            var firstAddress = agent.AgentHost.Endpoint.Address;
            Assert.That(server.HasAConnected(agent));

            agent.ShutDownUngraceful();
            agent = fixture.StartAgent("Agent #1 Name 1");

            Assert.That(firstAddress, Is.Not.EqualTo(agent.AgentHost.Endpoint.Address));
            Assert.That(server.HasAConnected("Agent #1 Name 1", agent.AgentHost.Endpoint.Address));
        }
    }
}