using System;
using System.Collections.Generic;
using System.Linq;
using NDistribUnit.Common.Common.Extensions;
using NDistribUnit.Common.Contracts.DataContracts;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="TestReprocessor"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public TestReprocessor(ITestUnitsCollection collection)
        {
            this.collection = collection;
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
                if (childResult != null && ProcessResult(childResult, test.Test.Run, test))
                {
                    AddTestForReprocessing(test, agent);
                }
                return;
            }


            foreach (var suiteResult in result.FindBottomLevelTestSuites())
            {
                if (test.FullName.Equals(suiteResult.FullName)
                    && ProcessResult(suiteResult, test.Test.Run, test))
                {
                    AddTestForReprocessing(test, agent);
                    continue;
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
                            })) 
                        
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

        private bool ProcessResult(TestResult result, TestRun testRun, TestUnitWithMetadata test,
                                   Func<TestUnitWithMetadata> createTestUnit = null)
        {
            if (result.ResultState.IsOneOf(
                ResultState.Ignored,
                ResultState.Success,
                ResultState.Cancelled))
                return false;

            var specialHandling = testRun.Parameters.GetSpecialHandling(result.Message, result.StackTrace);

            if (specialHandling == null)
                return false;

            test = test ?? (createTestUnit != null
                                ? createTestUnit()
                                : null);

            if (test == null)
                return false;

            return test.AttachedData.GetCountAndIncrease(specialHandling) <= specialHandling.RetryCount;
        }

        private void AddTestForReprocessing(TestUnitWithMetadata test, AgentMetadata agent)
        {
            test.AttachedData.MarkAgentAs(agent, SchedulingHint.NotRecommended);
            collection.Add(test);
        }
    }
}