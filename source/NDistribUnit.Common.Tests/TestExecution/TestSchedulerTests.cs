using System;
using System.ServiceModel;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Server;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.TestExecution.Exceptions;
using NDistribUnit.Common.TestExecution.Storage;
using NUnit.Framework;

namespace NDistribUnit.Common.Tests.TestExecution
{
    [TestFixture]
    public class TestSchedulerTests
    {
        private readonly Guid defaultId = Guid.NewGuid();
        private TestAgentsCollection agents;
        private TestUnitsCollection tests;
        private TestsScheduler scheduler;
        private RequestsStorage requests;

        [SetUp]
        public void Init()
        {
            agents = new TestAgentsCollection();
            tests = new TestUnitsCollection();
            requests = new RequestsStorage(new ServerConfiguration {PingIntervalInMiliseconds = 10000}, new ConsoleLog());

            scheduler = new TestsScheduler(agents, tests, requests);
        }
        
        [Test]
        public void GetBothReturnsNullIfNoTestsAreAvailable()
        {
            agents.Add(CreateTestAgent("first"));
            agents.Add(CreateTestAgent("second"));

            var tuple = scheduler.GetAgentAndTest();
            Assert.That(tuple, Is.Null);
        }

        [Test]
        public void GetBothReturnsNullIfNoAgentsAndTestsAreAvailable()
        {
            var tuple = scheduler.GetAgentAndTest();
            Assert.That(tuple, Is.Null);
        }

        [Test]
        public void GetBothReturnsNullIfTestsAreAvailableAndSomeAgentsAreBusy()
        {
            tests.Add(CreateTestUnit("first"));
            agents.Add(CreateTestAgent("first", AgentState.Busy));

            var tuple = scheduler.GetAgentAndTest();
            Assert.That(tuple, Is.Null);
        }

        [Test]
        public void GetBothReturnsNullIfTestsAreAvailableAndSomeAgentsAreUpdating()
        {
            tests.Add(CreateTestUnit("first"));
            agents.Add(CreateTestAgent("first", AgentState.Updating));

            var tuple = scheduler.GetAgentAndTest();
            Assert.That(tuple, Is.Null);
        }

        [Test]
        public void GetBothThrowsExceptionIfNoAgentsAreAvailableWhileTestsAre()
        {
            tests.Add(CreateTestUnit("first"));

            Assert.Throws<NoAvailableAgentsException>(() => scheduler.GetAgentAndTest());
        }

        private TestUnitWithMetadata CreateTestUnit(string testName, Guid? runId = null)
        {
            return new TestUnitWithMetadata(new TestRun
                                    {
                                        Id = runId ?? defaultId
                                    }, testName);
        }

        private AgentInformation CreateTestAgent(string agentName = null, AgentState state = AgentState.Ready)
        {
            return new AgentInformation
                       {
                           Name = agentName ?? Guid.NewGuid().ToString(),
                           State = state,
                           Address = new EndpointAddress(string.Format("http://{0}", agentName))
                       };
        }
    }
}