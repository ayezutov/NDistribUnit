using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Common.ConsoleProcessing;
using NDistribUnit.Common.Common.Domains;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.TestExecution.Data;
using NDistribUnit.Common.TestExecution.Storage;
using NDistribUnit.Common.Updating;
using NUnit.Core;
using NUnit.Util;
using AssemblyResolver = NDistribUnit.Common.Common.AssemblyResolver;
using DomainManager = NDistribUnit.Common.Common.Domains.DomainManager;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public class TestsRetriever : ITestsRetriever
    {
        private readonly ITestSystemInitializer initializer;
        private readonly BootstrapperParameters parameters;
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestsRetriever"/> class.
        /// </summary>
        /// <param name="initializer">The initializer.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="log">The log.</param>
        public TestsRetriever(ITestSystemInitializer initializer, BootstrapperParameters parameters, ILog log)
        {
            this.initializer = initializer;
            this.parameters = parameters;
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
            package.AutoBinPath = false;
            package.PrivateBinPath = NUnit.Util.DomainManager.GetPrivateBinPath(
                parameters.RootFolder
                ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                new ArrayList(request.TestRun.NUnitParameters.AssembliesToTest)
                    {
                        Assembly.GetExecutingAssembly().Location
                    });
            package.BasePath = parameters.RootFolder;

            var domainManager = new NUnit.Util.DomainManager();

            // A separate domain is used to free the assemblies after tests retrieval
            var domain = domainManager.CreateDomain(package);
            
            try
            {
                DomainManager.AddResolverForPaths(domain, DomainManager.GetNUnitFolders(parameters));
                
                var testsRetrieverType = typeof (InAnotherDomainTestsRetriever);

                var testsRetriever = (InAnotherDomainTestsRetriever)
                        domain.CreateInstanceAndUnwrap(testsRetrieverType.Assembly.FullName, testsRetrieverType.FullName);

                return testsRetriever.Get(package, request.TestRun);
            }
            finally
            {
                DomainManager.UnloadDomain(domain);
            }
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
    }

    

    internal class InAnotherDomainTestsRetriever : MarshalByRefObject
    {
        public IList<TestUnitWithMetadata> Get(TestPackage package, TestRun testRun)
        {
            new NUnitInitializer().Initialize();
            var builder = new TestSuiteBuilder();
            TestSuite testSuite = builder.Build(package);
            var filter = new NUnitTestsFilter(testRun.NUnitParameters.IncludeCategories,
                                              testRun.NUnitParameters.ExcludeCategories,
                                              testRun.NUnitParameters.TestToRun);
            return ToTestUnitList(testSuite, filter, testRun);
        }

        private IList<TestUnitWithMetadata> ToTestUnitList(ITest test, ITestFilter filter, TestRun testRun)
        {
            var result = new List<TestUnitWithMetadata>();

            FindTestUnits(test, filter, result, testRun);

            return result;
        }


        private static void FindTestUnits(ITest test, ITestFilter filter,
                                          List<TestUnitWithMetadata> result, TestRun testRun, string assemblyName = null)
        {
            var assembly = test as TestAssembly;

            if (assembly != null)
                assemblyName = assembly.TestName.FullName;

            if (filter.Pass(test))
            {
                var isTestSuiteWithAtLeastOneTestMethod = (test.IsSuite && test.Tests != null && test.Tests.Count != 0 &&
                                                           !((ITest) test.Tests[0]).IsSuite);

//                string testToRun = testRun.NUnitParameters.TestToRun;

                if (!test.IsSuite || isTestSuiteWithAtLeastOneTestMethod)
//                    || (string.IsNullOrEmpty(testToRun) && isTestSuiteWithAtLeastOneTestMethod)
//                    || (!string.IsNullOrEmpty(testToRun) && test.TestName.FullName.StartsWith(testToRun) && isTestSuiteWithAtLeastOneTestMethod))
                {
                    List<TestUnitWithMetadata> subTests = null;
                    if (test.IsSuite && test.Tests != null)
                    {
                        subTests = new List<TestUnitWithMetadata>();
                        foreach (ITest child in test.Tests)
                        {
                            FindTestUnits(child, filter, subTests, testRun, assemblyName);
                        }
                    }
                    var testUnitWithMetadata = new TestUnitWithMetadata(testRun, test, assemblyName, subTests);
                    result.Add(testUnitWithMetadata);
                }
                else if ((test.Tests != null && test.Tests.Count > 0))
                {
                    foreach (ITest child in test.Tests)
                    {
                        FindTestUnits(child, filter, result, testRun, assemblyName);
                    }
                }
            }
        }
    }
}