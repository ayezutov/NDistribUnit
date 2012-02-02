using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Server.AgentsTracking;
using NUnit.Core;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITestReprocessor
    {
        /// <summary>
        /// Adds for reprocessing if required.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="result">The result.</param>
        /// <param name="agent"> </param>
        void AddForReprocessingIfRequired(TestUnitWithMetadata test, TestResult result, AgentMetadata agent);
    }
}