using System;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.TestExecution.DistributedConfiguration;

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
        Tuple<AgentInformation, TestUnitWithMetadata, DistributedConfigurationSubstitutions> GetAgentAndTestAndVariables();
    }
}