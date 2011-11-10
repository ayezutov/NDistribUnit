using System;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.TestExecution.Configuration;
using NDistribUnit.Common.TestExecution.Exceptions;
using System.Linq;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public class TestsScheduler : ITestsScheduler
    {
        private readonly TestAgentsCollection agents;
        private readonly TestUnitsCollection tests;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestsScheduler"/> class.
        /// </summary>
        /// <param name="agents">The agents.</param>
        /// <param name="tests">The tests.</param>
        public TestsScheduler(TestAgentsCollection agents, TestUnitsCollection tests)
        {
            this.agents = agents;
            this.tests = tests;
        }

        /// <summary>
        /// Gets the agent for test.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <returns></returns>
        public AgentInformation GetAgentForTest(TestUnit test)
        {
            lock (agents.SyncObject)
            {
                if (!agents.AreConnectedAvailable)
                    throw new NoAvailableAgentsException();

                var free = agents.GetFree();
                if (free != null && free.Count != 0)
                    return free[0];

                return null;
            }
        }

        /// <summary>
        /// Gets the test for agent.
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <returns></returns>
        public TestUnit GetTestForAgent(AgentInformation agent)
        {
            lock (tests.SyncObject)
            {
                var list = tests.GetAvailable();
                return list.OrderBy(t => t.Request.RequestTime).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the agent and test.
        /// </summary>
        /// <returns></returns>
        public Tuple<AgentInformation, TestUnit> GetAgentAndTest()
        {
            lock (tests.SyncObject)
            {
                var availableTests = tests.GetAvailable();
                if (availableTests.Count == 0)
                    return null;

                lock (agents.SyncObject)
                {
                    if (!agents.AreConnectedAvailable)
                        throw new NoAvailableAgentsException();

                    var freeAgents = agents.GetFree();

                    if (freeAgents.Count == 0)
                        return null;

                    return new Tuple<AgentInformation, TestUnit>(freeAgents[0], availableTests[0]);
                }
            }
        }

        /// <summary>
        /// Processes the special handling.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="agent">The agent.</param>
        /// <param name="handling">The handling.</param>
        public void ProcessSpecialHandling(TestUnit test, AgentInformation agent, TestRunFailureSpecialHandling handling)
        {}
    }
}