using System.Collections.Generic;
using System.IO;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.TestExecution.Data;
using NDistribUnit.Common.TestExecution.Storage;

using NUnit.Core;
using System.Linq;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public class TestsRetriever : ITestsRetriever
    {
        private readonly ITestSystemInitializer initializer;
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestsRetriever"/> class.
        /// </summary>
        /// <param name="initializer">The initializer.</param>
        /// <param name="log">The log.</param>
        public TestsRetriever(ITestSystemInitializer initializer, ILog log)
        {
            this.initializer = initializer;
            this.log = log;
        }

        /// <summary>
        /// Parses the specified project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public IEnumerable<TestUnitWithMetadata> Get(TestProject project, TestRunRequest request)
        {
            initializer.Initialize();

            var projectFileNameOnly = Path.GetFileName(request.TestRun.NUnitParameters.AssembliesToTest[0]);
            var projectFileNameMapped = Path.Combine(project.Path, projectFileNameOnly);
            
            var package = new TestPackage(projectFileNameMapped);
            var builder = new TestSuiteBuilder();
            
            var testSuite = builder.Build(package);
            var filter = new NUnitTestsFilter(request.TestRun.NUnitParameters.IncludeCategories,
                                 request.TestRun.NUnitParameters.ExcludeCategories);
            
            return ToTestUnitList(testSuite, request, filter);
        }

        private IEnumerable<TestUnitWithMetadata> ToTestUnitList(TestSuite testSuite, TestRunRequest request, ITestFilter filter)
        {
            var rawTestUnits = new List<ITest>();
            Find(testSuite, filter, rawTestUnits);

            return rawTestUnits.Select(raw => new TestUnitWithMetadata(request.TestRun, raw.TestName.FullName));
        }

        /// <summary>
        /// Finds the specified test.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="result">The result.</param>
        /// <param name="assemblyName">Name of the assembly, where the test belongs to.</param>
        private static void Find(ITest test, ITestFilter filter, ICollection<ITest> result, string assemblyName = null)
        {
            var ass = test as TestAssembly;
            var ns = test as NamespaceSuite;

            if (ass == null && ns == null && filter.Pass(test) && test.IsSuite &&
                (test.Tests == null || test.Tests.Count == 0 || !((ITest) test.Tests[0]).IsSuite))
            {
                result.Add(test);
                return;
            }
            if (!test.IsSuite || test.Tests == null) return;

            foreach (ITest innerTest in test.Tests)
            {
                Find(innerTest, filter, result, ass != null ? ass.TestName.FullName : null);
            }
        }
    }
}