using System;
using System.Collections.Generic;
using System.Linq;
using NDistribUnit.Common.Common.Extensions;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Server.AgentsTracking;
using NDistribUnit.Common.TestExecution.Scheduling;
using NUnit.Core;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// A reprocessing strategy
    /// </summary>
    public class TestReprocessor : ITestReprocessor
    {
        private readonly ITestUnitsCollection collection;
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestReprocessor"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="log">The log.</param>
        public TestReprocessor(ITestUnitsCollection collection, ILog log)
        {
            this.collection = collection;
            this.log = log;
        }

        /// <summary>
        /// Adds for reprocessing if required.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="result">The result.</param>
        /// <param name="agent"> </param>
        public void AddForReprocessingIfRequired(TestUnitWithMetadata test, TestResult result, AgentMetadata agent)
        {
            if (result == null)
            {
                test.AttachedData.NullReprocessingCount++;
                collection.Add(test);
                return;
            }

            if (!test.Test.Info.IsSuite)
            {
                var childResult = result.FindDescedant(d => d.FullName.Equals(test.FullName));
                if (childResult != null 
                    && ProcessResult(childResult, test.Test.Run, test) == ReprocessingVerdict.PerformReprocessing)
                {
                    AddTestForReprocessing(test, agent);
                }
                return;
            }


            foreach (var suiteResult in result.FindBottomLevelTestSuites())
            {
                if (test.FullName.Equals(suiteResult.FullName))
                {
                    var reprocessingVerdict = ProcessResult(suiteResult, test.Test.Run, test);
                    if (reprocessingVerdict == ReprocessingVerdict.MaximumCountWasReached)
                        return;
                    
                    if (reprocessingVerdict == ReprocessingVerdict.PerformReprocessing)
                    {
                        AddTestForReprocessing(test, agent);
                        continue;
                    }
                }

                var childrenForReprocessing = new List<Tuple<TestResult, TestUnitWithMetadata>>();
                foreach (TestResult childResult in suiteResult.Results)
                {
                    var childTest = test.Children.FirstOrDefault(ct => ct.FullName.Equals(childResult.FullName));
                    if (ProcessResult(childResult, test.Test.Run, childTest, 
                        () =>
                            {
                                childTest = new TestUnitWithMetadata(test.Test.Run, childResult.Test, test.Test.AssemblyName);
                                test.Children.Add(childTest);
                                return childTest;
                            })==ReprocessingVerdict.PerformReprocessing) 
                        
                        childrenForReprocessing.Add(new Tuple<TestResult, TestUnitWithMetadata>(childResult, childTest));
                }

                if (childrenForReprocessing.Count > 0 && childrenForReprocessing.Count == suiteResult.Results.Count)
                    AddTestForReprocessing(test, agent);
                else
                {
                    foreach (var childForReprocessing in childrenForReprocessing)
                    {
                        AddTestForReprocessing(childForReprocessing.Item2, agent);
                    }
                }
            }
        }

        private ReprocessingVerdict ProcessResult(TestResult result, TestRun testRun, TestUnitWithMetadata test,
                                   Func<TestUnitWithMetadata> createTestUnit = null)
        {
            if (result.ResultState.IsOneOf(
                ResultState.Ignored,
                ResultState.Success,
                ResultState.Cancelled))
                return ReprocessingVerdict.Skipped;

            var specialHandling = testRun.Parameters.GetSpecialHandling(result.Message, result.StackTrace);

            if (specialHandling == null)
                return ReprocessingVerdict.NoHandlingFound;

            test = test ?? (createTestUnit != null
                                ? createTestUnit()
                                : null);

            if (test == null)
                return ReprocessingVerdict.Other;

            return test.AttachedData.GetCountAndIncrease(specialHandling) <= specialHandling.RetryCount
                ? ReprocessingVerdict.PerformReprocessing 
                : ReprocessingVerdict.MaximumCountWasReached;
        }

        private void AddTestForReprocessing(TestUnitWithMetadata test, AgentMetadata agent)
        {
            log.Info(string.Format("REPROCESSING: '{0}' was added for reprocessing. [{1}]", test.FullName, test.Test.Run));
            test.AttachedData.MarkAgentAs(agent, SchedulingHint.NotRecommended);
            collection.Add(test);
        }

        private enum ReprocessingVerdict
        {
            NoHandlingFound,
            Skipped,
            MaximumCountWasReached,
            PerformReprocessing,
            Other
        }
    }
}