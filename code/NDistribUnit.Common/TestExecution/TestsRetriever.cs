using System.Collections.Generic;
using System.IO;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.TestExecution.Data;
using NDistribUnit.Common.TestExecution.Storage;
using NUnit.Core;
using NUnit.Util;

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
        public IList<TestUnitWithMetadata> Get(TestProject project, TestRunRequest request)
        {
            initializer.Initialize();

            var projectFileNameOnly = Path.GetFileName(request.TestRun.NUnitParameters.AssembliesToTest[0]);
            var projectFileNameMapped = Path.Combine(project.Path, projectFileNameOnly);


            var package = GetTestPackage(projectFileNameMapped, request.TestRun.NUnitParameters);
            var builder = new TestSuiteBuilder();

            var testSuite = builder.Build(package);
            var filter = new NUnitTestsFilter(request.TestRun.NUnitParameters.IncludeCategories,
                                              request.TestRun.NUnitParameters.ExcludeCategories,
                                              request.TestRun.NUnitParameters.TestToRun);

            return ToTestUnitList(testSuite, request, filter);
        }

        private TestPackage GetTestPackage(string projectFileName, NUnitParameters nUnitParameters)
        {
            if (!NUnitProject.IsNUnitProjectFile(projectFileName))
                return new TestPackage(projectFileName);


            var nunitProject = new NUnitProject(projectFileName);
            nunitProject.Load();
            if (!string.IsNullOrEmpty(nUnitParameters.Configuration))
                nunitProject.SetActiveConfig(nUnitParameters.Configuration);

            return nunitProject.ActiveConfig.MakeTestPackage();
        }

        private IList<TestUnitWithMetadata> ToTestUnitList(ITest test, TestRunRequest request, ITestFilter filter)
        {
            var result = new List<TestUnitWithMetadata>();

            FindTestUnits(test, request, filter, result);

            return result;
        }

        private static void FindTestUnits(ITest test, TestRunRequest request, ITestFilter filter,
                                          List<TestUnitWithMetadata> result, string assemblyName = null)
        {
            var assembly = test as TestAssembly;

            if (assembly != null)
                assemblyName = assembly.TestName.FullName;

            if (filter.Pass(test))
            {
                
                var isTestSuiteWithAtLeastOneTestMethod = (test.IsSuite && test.Tests != null && test.Tests.Count != 0 && !((ITest) test.Tests[0]).IsSuite);

                string testToRun = request.TestRun.NUnitParameters.TestToRun;
                
                if ((string.IsNullOrEmpty(testToRun) && (isTestSuiteWithAtLeastOneTestMethod || !test.IsSuite))
                    || (!string.IsNullOrEmpty(testToRun) && test.TestName.FullName.StartsWith(testToRun)))
                {
                    List<TestUnitWithMetadata> subTests = null;
                    if (test.IsSuite && test.Tests != null)
                    {
                        subTests = new List<TestUnitWithMetadata>();
                        foreach (ITest child in test.Tests)
                        {
                            FindTestUnits(child, request, filter, subTests, assemblyName);
                        }
                    }
                    var testUnitWithMetadata = new TestUnitWithMetadata(request.TestRun, test, assemblyName, subTests);
                    result.Add(testUnitWithMetadata);
                }
                else if ((test.Tests != null && test.Tests.Count > 0))
                {
                    foreach (ITest child in test.Tests)
                    {
                        FindTestUnits(child, request, filter, result, assemblyName);
                    }
                }
            }
        }

//        /// <summary>
//        /// Finds the specified test.
//        /// </summary>
//        /// <param name="test">The test.</param>
//        /// <param name="filter">The filter.</param>
//        /// <param name="result">The result.</param>
//        /// <param name="assemblyName">Name of the assembly, where the test belongs to.</param>
//        private static void Find(ITest test, ITestFilter filter, ICollection<ITest> result, string assemblyName = null)
//        {
//            var ass = test as TestAssembly;
//            var ns = test as NamespaceSuite;
//
//            if (ass == null && ns == null && filter.Pass(test) && test.IsSuite &&
//                (test.Tests == null || test.Tests.Count == 0 || !((ITest) test.Tests[0]).IsSuite))
//            {
//                result.Add(test);
//                return;
//            }
//            if (!test.IsSuite || test.Tests == null) return;
//
//            foreach (ITest innerTest in test.Tests)
//            {
//                Find(innerTest, filter, result, ass != null ? ass.TestName.FullName : null);
//            }
//        }
    }
}