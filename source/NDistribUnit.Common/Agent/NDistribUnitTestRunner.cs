using System;
using System.IO;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.TestExecution.Storage;
using NUnit.Core;
using NUnit.Util;
using TestResult = NDistribUnit.Common.Contracts.DataContracts.TestResult;

namespace NDistribUnit.Common.Agent
{
    /// <summary>
    /// 
    /// </summary>
    public class NDistribUnitTestRunner
    {
        private readonly IProjectsStorage projects;
        private readonly INativeRunnerCache runnerCache;
        private readonly ITestSystemInitializer initializer;
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="NDistribUnitTestRunner"/> class.
        /// </summary>
        /// <param name="projects">The storage.</param>
        /// <param name="runnerCache">The cash.</param>
        /// <param name="initializer">The initializer.</param>
        /// <param name="log">The log.</param>
        public NDistribUnitTestRunner(
            IProjectsStorage projects,
            INativeRunnerCache runnerCache,
            ITestSystemInitializer initializer, 
            ILog log)
        {
            this.projects = projects;
            this.runnerCache = runnerCache;
            this.initializer = initializer;
            this.log = log;
        }

        /// <summary>
        /// Runs the specified test.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="dataSource">The data source.</param>
        /// <returns></returns>
        public TestResult Run(TestUnit test, IAgentDataSource dataSource)
        {
            var project = projects.GetProject(test.Request.TestRun);
            if (project == null)
            {
                log.Info("Sending a request for project files...");
                PackedProject packedProject = dataSource.GetPackedProject(test.Request.TestRun);
                if (packedProject == null)
                    return CreateInvalidTestResult(test);
                log.Info("Storing project...");
                project = projects.Store(test.Request.TestRun, packedProject);
            }

            var nativeRunner = runnerCache.GetOrLoad(test.Request.TestRun,
                                                     () =>
                                                         {
                                                             initializer.Initialize();
                                                             var mappedAssemblyFile = Path.Combine(project.Path, Path.GetFileName(test.Request.TestRun.TestOptions.AssembliesToTest[0]));
                                                             var package = new TestPackage(mappedAssemblyFile);
                                                             package.Settings["ShadowCopyFiles"] = true;
                                                             package.BasePath = package.PrivateBinPath = project.Path;
                                                             package.ConfigurationFile = mappedAssemblyFile+".config";
                                                             var nativeTestRunner = new MultipleTestDomainRunner();
                                                             nativeTestRunner.Load(package);
                                                             return nativeTestRunner;
                                                         });

            ClientParameters testOptions = test.Request.TestRun.TestOptions;
            NUnit.Core.TestResult testResult = nativeRunner.Run(new NullListener(),
                                                                new NUnitTestsFilter(testOptions.IncludeCategories,
                                                                                     testOptions.ExcludeCategories,
                                                                                     test.UniqueTestId));

            return MapResult(testResult, test);
        }

        private TestResult MapResult(NUnit.Core.TestResult testResult, TestUnit test)
        {
            throw new NotImplementedException();
        }

        private TestResult CreateInvalidTestResult(TestUnit test)
        {
            return new TestResult("Invalid test:" + test.UniqueTestId);
        }
    }
}