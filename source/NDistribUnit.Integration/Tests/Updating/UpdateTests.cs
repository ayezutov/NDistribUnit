﻿using System;
using NDistribUnit.Integration.Tests.Infrastructure;
using NUnit.Framework;

namespace NDistribUnit.Integration.Tests.Tests.Updating
{
    [TestFixture]
    public class UpdateTests
    {
        
        private NDistribUnitTestSystem system;

        [SetUp]
        public void Init()
        {
            system = new NDistribUnitTestSystem();
        }

        [TearDown]
        public void Dispose()
        {
            system.Dispose();
        }

        [TestFixtureSetUp]
        public void InitOnce()
        {

        }

        [TestFixtureTearDown]
        public void DisposeOnce()
        {
        }

        [Test]
        public void ServerIsRestartedIfUpdateWasFound()
        {
            Assert.Ignore("Cannot be tested in current architecture");
            //var server = system
            //    .StartServer();
            //
            //Assert.That(server.WasRestarted);
        }

        [Test]
        public void AgentReceivesUpdateIfAvailableOnServer()
        {
            var server = system
                .OfVersion(Version.Parse("2.0.0.0"))
                .StartServer();

            var agent = system
                .OfVersion(Version.Parse("1.9.0.0"))
                .StartAgent();

            Assert.That(agent.UpdateReceiver.HasReceivedUpdate(Version.Parse("2.0.0.0")));
        }

        [Test]
        public void AgentReceivesNoUpdateIfOfLowerVersionOnServer()
        {
            var server = system
                .OfVersion(Version.Parse("1.8.0.0"))
                .StartServer();

            var agent = system
                .OfVersion(Version.Parse("1.9.0.0"))
                .StartAgent();

            Assert.That(agent.UpdateReceiver.HasReceivedUpdate(), Is.False);
        }

        [Test]
        public void AgentReceivesNoUpdateIfOfSameVersionOnServer()
        {
            var server = system
                .OfVersion(Version.Parse("1.9.0.0"))
                .StartServer();

            var agent = system
                .OfVersion(Version.Parse("1.9.0.0"))
                .StartAgent();

            Assert.That(agent.UpdateReceiver.HasReceivedUpdate(), Is.False);
        }


        [Test]
        public void ClientReceivesUpdateIfAvailableOnServer()
        {
            var server = system
                .OfVersion(Version.Parse("2.0.0.0"))
                .StartServer();

            var client = system
                .OfVersion(Version.Parse("1.9.0.0"))
                .StartClient();

            Assert.That(client.HasReceivedUpdate(Version.Parse("2.0.0.0")));
        }
    }
}