using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NDistribUnit.Common.TestExecution.Data;
using NDistribUnit.Common.TestExecution.Storage;

using NUnit.Core;
using NUnit.Core.Filters;
using NUnit.Util;
using System.Linq;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public class TestsRetriever : ITestsRetriever
    {
        private readonly ITestSystemInitializer initializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestsRetriever"/> class.
        /// </summary>
        /// <param name="initializer">The initializer.</param>
        public TestsRetriever(ITestSystemInitializer initializer)
        {
            this.initializer = initializer;
        }

        /// <summary>
        /// Parses the specified project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public IEnumerable<TestUnit> Get(TestProject project, TestRunRequest request)
        {
            initializer.Initialize();

            var projectFileNameOnly = Path.GetFileName(request.TestRun.TestOptions.AssembliesToTest[0]);
            var projectFileNameMapped = Path.Combine(project.Path, projectFileNameOnly);
            
            var package = new TestPackage(projectFileNameMapped);
            var builder = new TestSuiteBuilder();
            
            var testSuite = builder.Build(package);
            var filter = new NUnitTestsFilter(request.TestRun.TestOptions.IncludeCategories,
                                 request.TestRun.TestOptions.ExcludeCategories);
            
            return ToTestUnitList(testSuite, request, filter);
        }

        private IEnumerable<TestUnit> ToTestUnitList(TestSuite testSuite, TestRunRequest request, ITestFilter filter)
        {
            var rawTestUnits = new List<ITest>();
            Find(testSuite, filter, rawTestUnits);

            return rawTestUnits.Select(tu => new TestUnit(request, tu.TestName.FullName));
        }

        private static void Find(ITest test, ITestFilter filter, ICollection<ITest> result)
        {
            System.Console.WriteLine(test.TestName.FullName);

            var ass = test as TestAssembly;
            var ns = test as NamespaceSuite;
            var tf = test as TestFixture;

            System.Console.WriteLine(tf);
            if (ass == null && ns == null)
            {
                if (filter.Pass(test))
                {
                    if (test.IsSuite)
                    {
                        if ((test.Tests == null || test.Tests.Count == 0 || !((ITest) test.Tests[0]).IsSuite))
                        {
                            result.Add(test);
                            return;
                        }
                    }
                }
            }
            if (!test.IsSuite || test.Tests == null) return;

            foreach (ITest innerTest in test.Tests)
            {
                Find(innerTest, filter, result);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class NUnitTestsFilter : ITestFilter
    {
        private readonly TestFilter filter;

        /// <summary>
        /// Initializes a new instance of the <see cref="NUnitTestsFilter"/> class.
        /// </summary>
        /// <param name="include">The include parameter.</param>
        /// <param name="exclude">The exclude parameter.</param>
        /// <param name="run">The run parameter.</param>
        public NUnitTestsFilter(string include, string exclude, string run = null)
        {
            var filters = new List<TestFilter>();
            
            if (!string.IsNullOrEmpty(run))
                filters.Add(new SimpleNameFilter(run));

            if (!string.IsNullOrEmpty(include))
                filters.Add(new CategoryExpression(include).Filter);

            if (!string.IsNullOrEmpty(exclude))
                filters.Add(new NotFilter(new CategoryExpression(exclude).Filter));

            if (filters.Count == 0)
                filter = TestFilter.Empty;
            else if (filters.Count == 1)
                filter = filters[0];
            else
                filter = new AndFilter(filters.ToArray());
            
            if (filter is NotFilter)
                ((NotFilter)filter).TopLevel = true;
        }

        /// <summary>
        /// Passes the specified test.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <returns></returns>
        public bool Pass(ITest test)
        {
            return filter.Pass(test);
        }

        /// <summary>
        /// Matches the specified test.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <returns></returns>
        public bool Match(ITest test)
        {
            return filter.Match(test);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty
        {
            get { return filter.IsEmpty; }
        }
    }
}