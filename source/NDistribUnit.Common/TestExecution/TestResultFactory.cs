using System;
using System.Collections;
using System.Diagnostics;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.TestExecution.Data;
using NUnit.Core;
using System.Linq;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public class TestResultFactory
    {
        private static class TestType
        {
            public const string Project = "Project";
            public const string Assembly = "Assembly";
            public const string Namespace = "Namespace";
            public const string TestFixture = "TestFixture";
            public const string TestMethod = "TestMethod";
        }

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
            return GetResult(exception, description, TestType.Project, string.Empty);
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
            return GetResultForTest(test, exception);
        }

        /// <summary>
        /// Gets the no available agents failure.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public static TestResult GetNoAvailableAgentsFailure(TestUnitWithMetadata test, Exception exception)
        {
            var result = GetResultForTest(test.Test, exception);
            TestResult suite = result;

            while (suite.HasResults)
            {
                suite = (TestResult)suite.Results[0];
            }

            // A test can be either test suite or test case
            // Here it is a suite
            if (test.Test.Info.IsSuite && test.Children.Any())
            {
                foreach (var child in test.Children)
                {
                    var childResult = new TestResult(child.Test.Info);
                    childResult.SetResult(ResultState.NotRunnable, exception, FailureSite.Parent);
                    suite.AddResult(childResult);
                }
            }

            return result;
        }

        private static TestResult GetResultForTest(TestUnit test, Exception exception)
        {
            var description = string.Format("There was an error, when running '{0}'", test.UniqueTestId);

            var projectResult = GetResult(exception, description, TestType.Project, string.Empty);
            var assemblyResult = GetResult(exception, description, TestType.Assembly, test.AssemblyName);
            projectResult.AddResult(assemblyResult);

            var previous = assemblyResult;
            var splittedTestName = test.UniqueTestId.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            // Add namespace parts
            int position;
            for (position = 0; position < splittedTestName.Length - (test.Info.IsSuite ? 1: 2); position++)
            {
                var namespacePart = GetResult(exception, description, TestType.Namespace, string.Join(".", splittedTestName, 0, position + 1));

                previous.AddResult(namespacePart);
                previous = namespacePart;
            }

            // Add test suite
            TestResult suiteResult = GetResult(exception, description, TestType.TestFixture, string.Join(".", splittedTestName, 0, position + 1));
            previous.AddResult(suiteResult);

            // Add test case, if it is not a suite
            if (!test.Info.IsSuite)
                suiteResult.AddResult(GetResult(exception, description, TestType.TestMethod, test.UniqueTestId, false));

            return projectResult;
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