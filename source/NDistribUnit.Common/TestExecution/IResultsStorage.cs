using NDistribUnit.Common.Contracts.DataContracts;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public interface IResultsStorage
    {
        /// <summary>
        /// Adds the specified result.
        /// </summary>
        /// <param name="test"></param>
        /// <param name="result">The result.</param>
        void Add(TestUnitWithMetadata test, TestResult result);

        /// <summary>
        /// Stores as completed.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <returns></returns>
        TestResult StoreAsCompleted(TestRun testRun);
    }
}