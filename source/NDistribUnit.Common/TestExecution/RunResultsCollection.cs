using System;
using System.Collections.Generic;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Logging;
using NUnit.Core;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
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
                    MergedResult = result.Result;
                else
                {
                    unmerged.Add(result);
                    Merge(async: true);
                }
            }
        }

        private void Merge(bool async)
        {
            if (unmerged.Count == 0)
                return;

            if (async)
            {
                new Action(() =>
                               {
                                   try
                                   {
                                       lock (syncObject)
                                       {
                                           Merge(false);
                                           if (unmerged.Count > 0)
                                               Merge(true);
                                       }
                                   }
                                   catch(Exception ex)
                                   {
                                       log.Warning("An error occurred while merging results", ex);
                                       throw;
                                   }
                               }).BeginInvoke(null, null);
                return;
            }

            
                if (unmerged.Count == 0)
                    return;

                var resultToMerge = unmerged[0];
                unmerged.Remove(resultToMerge);
                processor.Merge(resultToMerge, ref mergedResult);
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        /// <returns></returns>
        public TestResult Close()
        {
            closed = true;
            Merge(async: false);
            return MergedResult;
        }
    }
}