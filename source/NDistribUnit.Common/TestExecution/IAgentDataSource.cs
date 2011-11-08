using NDistribUnit.Common.Contracts.DataContracts;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAgentDataSource
    {
        /// <summary>
        /// Gets the packed project.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <returns></returns>
        PackedProject GetPackedProject(TestRun testRun);
    }
}