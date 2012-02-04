using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NDistribUnit.Common.Common.Extensions;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Server.AgentsTracking;
using NDistribUnit.Common.TestExecution.Scheduling;
using NDistribUnit.Common.TestExecution.Storage;
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
        private readonly RequestsStorage requests;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestReprocessor"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="log">The log.</param>
        /// <param name="requests">The requests.</param>
        public TestReprocessor(ITestUnitsCollection collection, ILog log, RequestsStorage requests)
        {
            this.collection = collection;
            this.log = log;
            this.requests = requests;
        }

        /// <summary>
        /// Adds for reprocessing if required.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="result">The result.</param>
        /// <param name="agent"> </param>
        public void AddForReprocessingIfRequired(TestUnitWithMetadata test, TestResult result, AgentMetadata agent)
        {
            var request = requests != null ? requests.GetBy(test.Test.Run) : null;
            Action registerReprocessing = () => { if (request != null) request.Statistics.RegisterReprocessing(); };

            if (result == null)
            {
                test.AttachedData.NullReprocessingCount++;
                collection.Add(test);
                log.Info(string.Format("REPROCESSING (as null): '{0}' was added for reprocessing. [{1}]",
                   test.FullName, test.Test.Run));
                registerReprocessing();
                return;
            }

            if (!test.Test.Info.IsSuite)
            {
                var childResult = result.FindDescedant(d => d.FullName.Equals(test.FullName));
                if (childResult != null 
                    && ProcessResult(childResult, test.Test.Run, test) == ReprocessingVerdict.PerformReprocessing)
                {
                    AddTestForReprocessing(test, agent);
                    registerReprocessing();
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
                        registerReprocessing();
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
                {
                    AddTestForReprocessing(test, agent);
                    registerReprocessing();
                }
                else
                {
                    foreach (var childForReprocessing in childrenForReprocessing)
                    {
                        AddTestForReprocessing(childForReprocessing.Item2, agent);
                        registerReprocessing();
                    }
                }
            }
        }

        private ReprocessingVerdict ProcessResult(TestResult result, TestRun testRun, TestUnitWithMetadata test,
                                   Func<TestUnitWithMetadata> createTestUnit = null)
        {
            if (result == null)
                return ReprocessingVerdict.Other;

            if (result.ResultState.IsOneOf(
                ResultState.Ignored,
                ResultState.Success,
                ResultState.Cancelled))
                return ReprocessingVerdict.Skipped;

            var specialHandling = testRun.Parameters.GetSpecialHandling(result.Message, result.StackTrace);

            if (specialHandling == null)
                return ReprocessingVerdict.NoHandlingFound;

            test = (test ?? (createTestUnit != null
                                ? createTestUnit()
                                : null));

            if (test == null)
                return ReprocessingVerdict.Other;

            var doReprocess = test.AttachedData.GetCountAndIncrease(specialHandling) <= specialHandling.RetryCount;

            if (doReprocess)
            {
                log.Info(string.Format("REPROCESSING ({2}/{3}): '{0}' was added for reprocessing. [{1}]", 
                    test.FullName, test.Test.Run, test.AttachedData.GetCount(specialHandling), specialHandling.RetryCount));
            }

            return doReprocess
                ? ReprocessingVerdict.PerformReprocessing 
                : ReprocessingVerdict.MaximumCountWasReached;
        }

        private void AddTestForReprocessing(TestUnitWithMetadata test, AgentMetadata agent)
        {
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