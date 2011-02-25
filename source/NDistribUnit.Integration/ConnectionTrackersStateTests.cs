using System;
using System.Diagnostics;
using System.Threading;
using NDistribUnit.Integration.Tests.General;
using NUnit.Framework;

namespace NDistribUnit.Integration.Tests
{
    [TestFixture]
    public class ConnectionTrackersStateTests: IntegrationTestBase
    {
        [Test]
        public void AgentIsNotConnectedByDefault()
        {
            var agent = AgentWrapper.Start();

            Assert.That(agent.IsNotConnectedTo(ServerWrapper.Any));
        }


        [Test]
        public void ServerIsNotConnectedByDefault()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var server = ServerWrapper.Start();
            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);

            Assert.That(server.IsNotConnectedTo(AgentWrapper.Any));
        }

        [Test]
        public void AgentAndServerAreConnectedIfServerIsStartedFirst()
        {
            var server = ServerWrapper.Start();
            var agent = AgentWrapper.Start();

            Assert.That(agent.IsConnectedTo(server));
            Assert.That(server.IsConnectedTo(agent));
        }

        [Test]
        public void AgentAndServerAreConnectedIfAgentIsStartedFirst()
        {
            var server = ServerWrapper.Start();
            var agent = AgentWrapper.Start();

            Thread.Sleep(2000);

            Assert.That(agent.IsConnectedTo(server));
            Assert.That(server.IsConnectedTo(agent));
        }

        [Test]
        public void AgentDetectsServersExpectedShutdown()
        {
            var server = ServerWrapper.Start();
            var agent = AgentWrapper.Start();

            Assert.That(agent.IsConnectedTo(server));
            Assert.That(server.IsConnectedTo(agent));

            server.ShutDownInExpectedWay();

            Assert.That(agent.IsNotConnectedTo(server));
        }

        [Test]
        public void AgentDetectsServersUnexpectedShutdown()
        {
            var server = ServerWrapper.Start();
            var agent = AgentWrapper.Start();

            Assert.That(agent.IsConnectedTo(server));
            Assert.That(server.IsConnectedTo(agent));

            server.ShutDownUngraceful();

            Assert.That(agent.IsNotConnectedTo(server));
        }

        [Test]
        public void ServerDetectsAgentsExpectedShutdown()
        {
            var server = ServerWrapper.Start();
            var agent = AgentWrapper.Start();

            Assert.That(agent.IsConnectedTo(server));
            Assert.That(server.IsConnectedTo(agent));

            agent.ShutDownInExpectedWay();

            Assert.That(server.IsNotConnectedTo(agent));
        }

        [Test]
        public void ServerDetectsAgentsUnexpectedShutdown()
        {
            var server = ServerWrapper.Start();
            var agent = AgentWrapper.Start();

            Assert.That(agent.IsConnectedTo(server));
            Assert.That(server.IsConnectedTo(agent));

            agent.ShutDownUngraceful();

            Assert.That(server.IsNotConnectedTo(agent));
        }


    }

    public class IntegrationTestBase
    {
    }
}
