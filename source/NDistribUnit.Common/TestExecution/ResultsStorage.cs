using System;
using System.Collections.Concurrent;
using NDistribUnit.Common.Contracts.DataContracts;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// A default implementation of results storage
    /// </summary>
    public class ResultsStorage : IResultsStorage
    {
        private readonly ConcurrentDictionary<Guid, RunResultsCollection> results = new ConcurrentDictionary<Guid, RunResultsCollection>();

        /// <summary>
        /// Adds the specified result.
        /// </summary>
        /// <param name="test"></param>
        /// <param name="result">The result.</param>
        public void Add(TestUnitWithMetadata test, TestResult result)
        {
            var resultsForTestRun = GetCollection(test.Test.Run);
            resultsForTestRun.AddUnmerged(result);
        }

        private RunResultsCollection GetCollection(TestRun testRun)
        {
            return results.GetOrAdd(testRun.Id, guid => new RunResultsCollection());
        }

        /// <summary>
        /// Stores as completed.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <returns></returns>
        public TestResult StoreAsCompleted(TestRun testRun)
        {
            return null;
        }
    }
}