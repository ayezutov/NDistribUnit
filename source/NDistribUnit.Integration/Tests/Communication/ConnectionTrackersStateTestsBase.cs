using System;
using NDistribUnit.Integration.Tests.Infrastructure;
using NUnit.Framework;

namespace NDistribUnit.Integration.Tests.Tests.Communication
{
    public abstract class ConnectionTrackersStateTestsBase
    {
        private NDistribUnitTestSystem system;

        [SetUp]
        public void Init()
        {
            system = new NDistribUnitTestSystem();
            ConfigureSystem(system);
        }

        protected abstract void ConfigureSystem(NDistribUnitTestSystem system);

        [TearDown]
        public void TearDown()
        {
            try
            {
                system.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        [Test]
        public void ServerIsNotConnectedByDefault()
        {
            var server = system.StartServer();
            
            Assert.That(server.HasNoConnectedAgents());
        }

        [Test]
        public void AgentAndServerAreConnectedIfServerIsStartedFirst()
        {
            var server = system.StartServer();
            var agent = system.StartAgent();

            Assert.That(server.HasAConnected(agent));
        }

        [Test]
        public void AgentAndServerAreConnectedIfAgentIsStartedFirst()
        {
            var agent = system.StartAgent();
            var server = system.StartServer();

            Assert.That(server.HasAConnected(agent));
        }

        [Test]
        public void ServerDetectsAgentsExpectedShutdown()
        {
            var server = system.StartServer();
            var agent = system.StartAgent();

            Assert.That(server.HasAConnected(agent));

            agent.ShutDownInExpectedWay();

            Assert.That(server.HasADisconnected(agent));
        }

        [Test]
        public void ServerDetectsAgentsUnexpectedShutdown()
        {
            var server = system.StartServer();
            var agent = system.StartAgent();

            if (agent.AgentHost == null)
            {
                Assert.Ignore("Test is ignored on real system emulation");
                return;
            }

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
            var server = system.StartServer();
            var agent = system.StartAgent("Agent #1 Name 1");

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
            var server = system.StartServer();
            var agent = system.StartAgent("Agent #1 Name 1");

            if (agent.AgentHost == null)
            {
                Assert.Ignore("Test is ignored on real system emulation");
                return;
            }

            var firstAddress = agent.AgentHost.Endpoint.Address;
            Assert.That(server.HasAConnected(agent));

            agent.ShutDownUngraceful();
            agent = system.StartAgent("Agent #1 Name 1");

            Assert.That(firstAddress, Is.Not.EqualTo(agent.AgentHost.Endpoint.Address));
            Assert.That(server.HasAConnected("Agent #1 Name 1", agent.AgentHost.Endpoint.Address));
        }
    }
}