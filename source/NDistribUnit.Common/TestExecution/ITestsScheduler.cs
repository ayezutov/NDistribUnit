using System;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.TestExecution.Configuration;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITestsScheduler
    {
        /// <summary>
        /// Gets the agent for test.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <returns></returns>
        AgentInformation GetAgentForTest(TestUnit test);

        /// <summary>
        /// Gets the test for agent.
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <returns></returns>
        TestUnit GetTestForAgent(AgentInformation agent);

        /// <summary>
        /// Gets the agent and test.
        /// </summary>
        /// <returns></returns>
        Tuple<AgentInformation, TestUnit> GetAgentAndTest();

        /// <summary>
        /// Processes the special handling.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="agent">The agent.</param>
        /// <param name="handling">The handling.</param>
        void ProcessSpecialHandling(TestUnit test, AgentInformation agent, TestRunFailureSpecialHandling handling);
    }
}