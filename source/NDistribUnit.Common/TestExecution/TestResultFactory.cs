using System;
using System.Collections;
using System.Diagnostics;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.TestExecution.Data;
using NUnit.Core;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public class TestResultFactory
    {
        /// <summary>
        /// Gets the project retrieval failure.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public static TestResult GetProjectRetrievalFailure(TestRunRequest test, Exception exception = null)
        {
            return GetResultForProject(test.TestRun, exception, TestResultType.ProjectRetrievalFailure);
        }

        /// <summary>
        /// Gets the no available test failure.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public static TestResult GetNoAvailableTestFailure(TestRunRequest request)
        {
            return GetResultForProject(request.TestRun, null, TestResultType.NoTests);
        }

        private static TestResult GetResultForProject(TestRun testRun, Exception exception,
                                                      TestResultType testResultType)
        {
            var description = string.Format("There was an error, when running '{0}' ({1})", testRun.Id, testResultType);
            return GetResult(exception, description, "Project", string.Empty);
        }

        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="description">The description.</param>
        /// <param name="testType">Type of the test.</param>
        /// <param name="fullName">The full name.</param>
        /// <param name="isSuite">if set to <c>true</c> [is suite].</param>
        /// <returns></returns>
        private static TestResult GetResult(Exception exception, string description, string testType, string fullName, bool isSuite = true)
        {
            var result = new TestResult(new TestDataProvider
                                            {
                                                TestName = new TestName
                                                               {
                                                                   FullName = fullName,
                                                                   Name = fullName
                                                               },
                                                IsSuite = isSuite,
                                                TestType = testType,
                                            });
            result.SetResult(ResultState.NotRunnable, 
                exception == null ? description : exception.Message,
                exception == null ? null : exception.StackTrace);

            return result;
        }

        /// <summary>
        /// Gets the project retrieval failure.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public static TestResult GetProjectRetrievalFailure(TestUnit test, Exception exception = null)
        {
            return GetResultForTest(test, exception, TestResultType.ProjectRetrievalFailure);
        }

        private static TestResult GetResultForTest(TestUnit test, Exception exception, TestResultType testResultType)
        {
            var description = string.Format("There was an error, when running '{0}'", test.UniqueTestId);
            var projectResult = GetResult(exception, description, "Project", string.Empty);
            var assemblyResult = GetResult(exception, description, "Assembly", test.AssemblyName);
            projectResult.AddResult(assemblyResult);
            assemblyResult.AddResult(GetNamespacedResultForTest(test, exception, testResultType, description));
            return projectResult;
        }

        private static TestResult GetNamespacedResultForTest(TestUnit test, Exception exception, TestResultType testResultType, string description)
        {
            Debug.Assert(!string.IsNullOrEmpty(test.UniqueTestId));
            TestResult result = null;
            TestResult last = null;
            var splitted = test.UniqueTestId.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < splitted.Length - 2; i++)
            {
                var current = GetResult(exception, description, "Namespace", string.Join(".", splitted, 0, i+1));
                if (last == null)
                    result = last = current;
                else
                {
                    last.AddResult(current);
                    last = current;
                }
            }

            var fixture = GetResult(exception, description, "TestFixture", string.Join(".", splitted, 0, splitted.Length - 1));
            var method  = GetResult(exception, description, "TestMethod",  test.UniqueTestId, false);
            fixture.AddResult(method);
            if (last == null)
                result = fixture;
            else
                last.AddResult(fixture);

            return result;
        }

        /// <summary>
        /// Gets the no available agents failure.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public static TestResult GetNoAvailableAgentsFailure(TestUnitWithMetadata test, Exception exception)
        {
            return GetResultForTest(test.Test, exception, TestResultType.NoAvailableAgent);
        }

        /// <summary>
        /// Gets the unhandled exception failure.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public static TestResult GetUnhandledExceptionFailure(TestRunRequest request, Exception exception)
        {
            return GetResultForProject(request.TestRun, exception, TestResultType.Unhandled);
        }
    }
}