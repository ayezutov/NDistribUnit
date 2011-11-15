using System;
using NDistribUnit.Common.Contracts.DataContracts;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITestsScheduler
    {
        /// <summary>
        /// Gets the agent and test.
        /// </summary>
        /// <returns></returns>
        Tuple<AgentInformation, TestUnitWithMetadata> GetAgentAndTest();
    }
}