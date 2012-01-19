using NDistribUnit.Common.Contracts.DataContracts;
using NUnit.Core;

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
        /// <param name="result">The result.</param>
        /// <param name="testRun"> </param>
        void Add(TestResult result, TestRun testRun);

        /// <summary>
        /// Stores as completed.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <returns></returns>
        TestResult StoreAsCompleted(TestRun testRun);

        /// <summary>
        /// Gets the completed result.
        /// </summary>
        /// <param name="testRun">The run.</param>
        /// <returns></returns>
        TestResult GetCompletedResult(TestRun testRun);
    }
}