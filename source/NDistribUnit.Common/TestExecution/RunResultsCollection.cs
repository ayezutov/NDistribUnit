using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Logging;
using NUnit.Core;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// Represents a collection of test results for a single test run
    /// </summary>
    public class RunResultsCollection
    {
        private readonly TestResultsProcessor processor;
        private readonly ILog log;
        private readonly object syncObject = new object();
        private readonly List<TestUnitResult> unmerged = new List<TestUnitResult>();
        private bool closed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RunResultsCollection"/> class.
        /// </summary>
        /// <param name="processor">The processor.</param>
        /// <param name="log">The log.</param>
        public RunResultsCollection(TestResultsProcessor processor, ILog log)
        {
            this.processor = processor;
            this.log = log;
        }

        private TestResult mergedResult;

        /// <summary>
        /// Gets the result.
        /// </summary>
        public TestResult MergedResult
        {
            get { return mergedResult; }
            private set { mergedResult = value; }
        }

        /// <summary>
        /// Adds the unmerged.
        /// </summary>
        /// <param name="result">The result.</param>
        public void AddUnmerged(TestUnitResult result)
        {
            if (closed)
                throw new InvalidOperationException("It is not allowed to add new results after finalization.");

            lock (syncObject)
            {
                if (MergedResult == null)
                    MergedResult = new TestResult(result.Result.Test);

                unmerged.Add(result);
                MergeAsync();
            }
        }

        private void MergeAsync()
        {
            Task.Factory.StartNew(() =>
                                      {
                                          try
                                          {
                                              lock (syncObject)
                                              {
                                                  MergeNext();
                                                  if (unmerged.Count > 0)
                                                      MergeAsync();
                                              }
                                          }
                                          catch (Exception ex)
                                          {
                                              log.Warning("An error occurred while merging results", ex);
                                              throw;
                                          }
                                      });
        }

        private void MergeSynchronously()
        {
            lock (syncObject)
            {
                while (unmerged.Count > 0)
                {
                    MergeNext();
                }
            }
        }

        private void MergeNext()
        {
            lock (syncObject)
            {
                if (unmerged.Count == 0)
                    return;

                var resultToMerge = unmerged[0];
                unmerged.Remove(resultToMerge);
                processor.Merge(resultToMerge, ref mergedResult);
            }
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        /// <returns></returns>
        public TestResult Close()
        {
            closed = true;
            MergeSynchronously();
            return MergedResult;
        }
    }
}