using System;
using NDistribUnit.Common.Server.AgentsTracking;
using NDistribUnit.Common.TestExecution.DistributedConfiguration;

namespace NDistribUnit.Common.TestExecution.Scheduling
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
        Tuple<AgentMetadata, TestUnitWithMetadata, DistributedConfigurationSubstitutions> GetAgentAndTestAndVariables();
    }
}