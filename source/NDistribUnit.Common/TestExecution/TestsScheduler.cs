using System;
using System.Collections.Generic;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Server.Communication;
using NDistribUnit.Common.TestExecution.Configuration;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public class TestsScheduler : ITestsScheduler
    {
        private IList<AgentInformation> agents;
        private TestUnitCollection tests;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestsScheduler"/> class.
        /// </summary>
        /// <param name="connectionsTracker">The connections tracker.</param>
        /// <param name="tests">The tests.</param>
        public TestsScheduler(ServerConnectionsTracker connectionsTracker, TestUnitCollection tests)
        {
            agents = connectionsTracker.Agents;
            this.tests = tests;
        }

        /// <summary>
        /// Gets the agent for test.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <returns></returns>
        public AgentInformation GetAgentForTest(TestUnit test)
        {
            
        }

        /// <summary>
        /// Gets the test for agent.
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <returns></returns>
        public TestUnit GetTestForAgent(AgentInformation agent)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the agent and test.
        /// </summary>
        /// <returns></returns>
        public Tuple<AgentInformation, TestUnit> GetAgentAndTest()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Processes the special handling.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="agent">The agent.</param>
        /// <param name="handling">The handling.</param>
        public void ProcessSpecialHandling(TestUnit test, AgentInformation agent, TestRunFailureSpecialHandling handling)
        {
            throw new NotImplementedException();
        }
    }
}