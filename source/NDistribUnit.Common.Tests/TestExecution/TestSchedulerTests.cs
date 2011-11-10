using System;
using System.ServiceModel;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.TestExecution.Data;
using NDistribUnit.Common.TestExecution.Exceptions;
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

        [SetUp]
        public void Init()
        {
            agents = new TestAgentsCollection();
            tests = new TestUnitsCollection();

            scheduler = new TestsScheduler(agents, tests);
        }
        
        [Test]
        public void GetAgentThrowsExceptionIfNoAgentsAreAvailable()
        {
            Assert.Throws<NoAvailableAgentsException>(()=>scheduler.GetAgentForTest(CreateTestUnit("first")));
        }


        [Test]
        public void GetAgentThrowsExceptionIfAllAgentsAreDisconnected()
        {
            agents.Add(CreateTestAgent("first", AgentState.Disconnected));
            agents.Add(CreateTestAgent("second", AgentState.Disconnected));

            Assert.Throws<NoAvailableAgentsException>(()=> scheduler.GetAgentForTest(CreateTestUnit("first")));
        }

        [Test]
        public void GetAgentThrowsExceptionIfAllAgentsAreError()
        {
            agents.Add(CreateTestAgent("first", AgentState.Error));
            agents.Add(CreateTestAgent("second", AgentState.Error));

            Assert.Throws<NoAvailableAgentsException>(()=> scheduler.GetAgentForTest(CreateTestUnit("first")));
        }

        [Test]
        public void GetAgentReturnsFirstIfThereIsAtLeastOneReadyAgent()
        {
            agents.Add(CreateTestAgent("first"));
            agents.Add(CreateTestAgent("second"));

            var agent = scheduler.GetAgentForTest(CreateTestUnit("first"));
            Assert.That(agent, Is.Not.Null);
            Assert.That(agent.Name, Is.EqualTo("first"));
        }

        [Test]
        public void GetAgentThrowsExceptionIfAllAgentsAreErrorAndDisconnected()
        {
            agents.Add(CreateTestAgent("first", AgentState.Error));
            agents.Add(CreateTestAgent("second", AgentState.Disconnected));

            Assert.Throws<NoAvailableAgentsException>(()=> scheduler.GetAgentForTest(CreateTestUnit("first")));
        }

        [Test]
        public void GetAgentReturnsNullIfAllAgentsAreBusy()
        {
            agents.Add(CreateTestAgent("first", AgentState.Busy));

            var agent = scheduler.GetAgentForTest(CreateTestUnit("first"));

            Assert.That(agent, Is.Null);
        }

        [Test]
        public void GetAgentReturnsNullIfAllAgentsAreUpdating()
        {
            agents.Add(CreateTestAgent("first", AgentState.Updating));

            var agent = scheduler.GetAgentForTest(CreateTestUnit("first"));

            Assert.That(agent, Is.Null);
        }
        
        [Test]
        public void GetTestReturnsFirstIfThereIsAtLeastOneAvailable()
        {
            tests.Add(CreateTestUnit("first"));
            tests.Add(CreateTestUnit("second"));
            tests.Add(CreateTestUnit("third"));

            var testUnit = scheduler.GetTestForAgent(CreateTestAgent("agent"));
            Assert.That(testUnit, Is.Not.Null);
            Assert.That(testUnit.UniqueTestId, Is.EqualTo("first"));
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
            
            Assert.Throws<NoAvailableAgentsException>(()=>scheduler.GetAgentAndTest());
        }

        private TestUnit CreateTestUnit(string testName, Guid? runId = null)
        {
            return new TestUnit(new TestRunRequest(new TestRun
                                                       {
                                                           Id = runId ?? defaultId
                                                       }, null), testName);
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