using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly List<TestResult> unmerged = new List<TestResult>();
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
        /// Occurs when all items are merged.
        /// </summary>
        public event EventHandler<EventArgs<TestResult>> AllItemsMerged;

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
        public void AddUnmerged(TestResult result)
        {
            if (closed)
                throw new InvalidOperationException("It is not allowed to add new results after finalization.");

            lock (syncObject)
            {
                if (MergedResult == null)
                    MergedResult = new TestResult(result.Test);

                unmerged.Add(result);
                MergeAsync();
            }
        }

        private Task task;
        private void MergeAsync()
        {
            lock (syncObject)
            {
                if (unmerged.Count == 0)
                    return;

                task = task == null || task.IsCompleted
                    ? Task.Factory.StartNew(PerformAsyncMerging) 
                    : task.ContinueWith(t => PerformAsyncMerging());
            }
        }

        private void PerformAsyncMerging()
        {
            lock (syncObject)
            {
                try
                {
                    if (unmerged.Count == 0)
                        return;

                    MergeNext();
                    if (unmerged.Count > 0)
                        MergeAsync();
                    else
                    {
                        var handler = AllItemsMerged;
                        if (handler != null)
                            handler(this, new EventArgs<TestResult>(MergedResult));
                    }
                }
                catch (Exception ex)
                {
                    log.Warning("An error occurred while merging results", ex);
                }
            }
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
                processor.Merge(resultToMerge, mergedResult);
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