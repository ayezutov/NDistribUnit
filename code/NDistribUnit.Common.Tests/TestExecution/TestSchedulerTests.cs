using System;
using System.ServiceModel;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Server;
using NDistribUnit.Common.Server.AgentsTracking;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.TestExecution.Exceptions;
using NDistribUnit.Common.TestExecution.Scheduling;
using NDistribUnit.Common.TestExecution.Storage;
using NUnit.Core;
using NUnit.Framework;

namespace NDistribUnit.Common.Tests.TestExecution
{
    [TestFixture]
    public class TestSchedulerTests
    {
        private readonly Guid defaultId = Guid.NewGuid();
        private AgentsCollection agents;
        private TestUnitsCollection tests;
        private TestsScheduler scheduler;
        private RequestsStorage requests;

        [SetUp]
        public void Init()
        {
            agents = new AgentsCollection(new ConsoleLog());
            tests = new TestUnitsCollection();
            requests = new RequestsStorage(new ServerConfiguration {PingIntervalInMiliseconds = 10000}, new ConsoleLog());

            scheduler = new TestsScheduler(agents, tests, requests);
        }
        
        [Test]
        public void GetBothReturnsNullIfNoTestsAreAvailable()
        {
            agents.Connect(CreateTestAgent("first"));
            agents.Connect(CreateTestAgent("second"));

            var tuple = scheduler.GetAgentAndTestAndVariables();
            Assert.That(tuple, Is.Null);
        }

        [Test]
        public void GetBothReturnsNullIfNoAgentsAndTestsAreAvailable()
        {
            var tuple = scheduler.GetAgentAndTestAndVariables();
            Assert.That(tuple, Is.Null);
        }

        [Test]
        public void GetBothReturnsNullIfTestsAreAvailableAndSomeAgentsAreBusy()
        {
            tests.Add(CreateTestUnit("first"));
            var agent = CreateTestAgent("first");
            agents.Connect(agent);
            agents.MarkAsBusy(agent);

            var tuple = scheduler.GetAgentAndTestAndVariables();
            Assert.That(tuple, Is.Null);
        }

        [Test]
        public void GetBothReturnsNullIfTestsAreAvailableAndSomeAgentsAreUpdating()
        {
            tests.Add(CreateTestUnit("first"));
            var agent = CreateTestAgent("first");
            agents.Connect(agent);
            agents.MarkAsUpdating(agent);

            var tuple = scheduler.GetAgentAndTestAndVariables();
            Assert.That(tuple, Is.Null);
        }

        [Test]
        public void GetBothThrowsExceptionIfNoAgentsAreAvailableWhileTestsAre()
        {
            tests.Add(CreateTestUnit("first"));

            Assert.Throws<NoAvailableAgentsException>(() => scheduler.GetAgentAndTestAndVariables());
        }

        private TestUnitWithMetadata CreateTestUnit(string testName, Guid? runId = null)
        {
            return new TestUnitWithMetadata(new TestRun
                                                {
                                                    Id = runId ?? defaultId
                                                }, new TestDataProvider
                                                       {
                                                           TestName = new TestName
                                                                          {
                                                                              FullName = testName,
                                                                              Name = testName,
                                                                          },
                                                           IsSuite = true,
                                                           TestType = "TestFixture",
                                                       }, "http://someName/assembly.dll");
        }

        private AgentMetadata CreateTestAgent(string agentName = null)
        {
            return new AgentMetadata(new EndpointAddress(string.Format("http://{0}", agentName)))
                       {
                           Name = agentName ?? Guid.NewGuid().ToString(),
                       };
        }
    }
}
