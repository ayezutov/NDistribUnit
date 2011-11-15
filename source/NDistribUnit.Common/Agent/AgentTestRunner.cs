using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public class AgentTestRunner
    {
        private readonly IProjectsStorage projects;
        private readonly INativeRunnerCache runnerCache;
        private readonly ITestSystemInitializer initializer;
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTestRunner"/> class.
        /// </summary>
        /// <param name="projects">The storage.</param>
        /// <param name="runnerCache">The cash.</param>
        /// <param name="initializer">The initializer.</param>
        /// <param name="log">The log.</param>
        public AgentTestRunner(
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
            var project = projects.GetOrLoad(test.Run);
            if (project == null)
            {
                log.Info("Sending a request for project files...");
                PackedProject packedProject = dataSource.GetPackedProject(test.Run);
                if (packedProject == null)
                    return CreateInvalidTestResult(test);
                log.Info("Storing project...");
                project = projects.Store(test.Run, packedProject);
            }

            var nativeRunner = runnerCache.GetOrLoad(test.Run,
                                                     () =>
                                                         {
                                                             initializer.Initialize();
                                                             var mappedAssemblyFile = Path.Combine(project.Path, Path.GetFileName(test.Run.NUnitParameters.AssembliesToTest[0]));
                                                             var package = new TestPackage(mappedAssemblyFile);
                                                             package.Settings["ShadowCopyFiles"] = true;
                                                             package.BasePath = package.PrivateBinPath = project.Path;
                                                             package.ConfigurationFile = mappedAssemblyFile+".config";
                                                             var nativeTestRunner = new MultipleTestDomainRunner();
                                                             nativeTestRunner.Load(package);
                                                             return nativeTestRunner;
                                                         });

            var testOptions = test.Run.NUnitParameters;
            try
            {
                NUnit.Core.TestResult testResult = nativeRunner.Run(new NullListener(),
                                                                    new NUnitTestsFilter(testOptions.IncludeCategories,
                                                                                         testOptions.ExcludeCategories,
                                                                                         test.UniqueTestId));
                return MapResult(testResult, test);
            }
            catch(Exception ex)
            {
                log.Error("Error while running test on agent", ex);
                throw;
            }
        }

        private TestResult MapResult(NUnit.Core.TestResult testResult, TestUnit test)
        {
            return new TestResult
                       {
                           AssertCount = testResult.AssertCount,
                           Description = testResult.Description,
                           FailureSite = testResult.FailureSite,
                           FullName = testResult.FullName,
                           Message = testResult.Message,
                           Name = testResult.Name,
                           ResultState = testResult.ResultState,
                           StackTrace = testResult.StackTrace,
                           Time = testResult.Time,
                           MachineNames = new List<string> {Environment.MachineName},
                           Results = testResult.Results == null
                                         ? null
                                         : new List<TestResult>(
                                               testResult.Results
                                                   .Cast<NUnit.Core.TestResult>()
                                                   .Select(r => MapResult(r, test)))

                       };
        }

        private TestResult CreateInvalidTestResult(TestUnit test)
        {
            return new TestResult(/*"Invalid test:" + test.UniqueTestId*/);
        }
    }
}