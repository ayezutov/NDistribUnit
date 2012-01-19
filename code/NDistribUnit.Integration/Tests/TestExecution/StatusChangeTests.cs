using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Integration.Tests.Infrastructure;
using NUnit.Framework;

namespace NDistribUnit.Integration.Tests.TestExecution
{
    [TestFixture]
    public class StatusChangeTests
    {
        private NDistribUnitTestSystemFluent system;

        [SetUp]
        public void Init()
        {
            system = new NDistribUnitTestSystemFluent();
        }

        [TearDown]
        public void Dispose()
        {
        }
        
        [Test]
        public void AgentIsDetectedByServer()
        {
            var agent = system.StartAgent();
            var server = system.StartServer();

            Assert.That(server.HasAgent(agent, AgentState.Ready));
        }

        [Test]
        public void AgentIsBusyWhileRunningTest()
        {
            var agent = system.StartAgent();
            var server = system.StartServer();

//            var project = system.CreateTestProject();
//            project.AddTest(t => t.Executes(()=>
//                                                {
//                                                    Assert.That(server.HasAgent(agent, AgentState.Busy));
//                                                }
//                                     ));
//
//            Assert.That(server.HasAgent(agent, AgentState.Ready));
//
//            server.RunTests(project);

            Assert.That(server.HasAgent(agent, AgentState.Ready));
        }

        [Test]
        public void AgentIsDisconnectedIfCommunicationExceptionThrown()
        {
            var agent = system.StartAgent();
            var server = system.StartServer();

//            var project = system.CreateTestProject();
//            project.AddTest(t => t.Executes(()=>
//                                                {
//                                                    Assert.That(server.HasAgent(agent, AgentState.Busy));
//                                                }
//                                     ));
//
//            Assert.That(server.HasAgent(agent, AgentState.Ready));
//
//            server.RunTests(project);

            Assert.That(server.HasAgent(agent, AgentState.Ready));
        }
    }
}