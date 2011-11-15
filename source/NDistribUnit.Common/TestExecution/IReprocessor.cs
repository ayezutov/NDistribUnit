using NDistribUnit.Common.Contracts.DataContracts;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public interface IReprocessor
    {
        /// <summary>
        /// Adds for reprocessing if required.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="result">The result.</param>
        void AddForReprocessingIfRequired(TestUnitWithMetadata test, TestResult result);
    }
}