using System;
using System.IO;
using System.Threading;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.TestExecution.DistributedConfiguration;
using NDistribUnit.Common.TestExecution.Storage;
using NUnit.Core;
using NUnit.Util;

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
        private readonly IDistributedConfigurationOperator configurationOperator;
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTestRunner"/> class.
        /// </summary>
        /// <param name="projects">The storage.</param>
        /// <param name="runnerCache">The cash.</param>
        /// <param name="initializer">The initializer.</param>
        /// <param name="configurationOperator">The configuration operator.</param>
        /// <param name="log">The log.</param>
        public AgentTestRunner(
            IProjectsStorage projects,
            INativeRunnerCache runnerCache,
            ITestSystemInitializer initializer,
            IDistributedConfigurationOperator configurationOperator,
            ILog log)
        {
            this.projects = projects;
            this.runnerCache = runnerCache;
            this.initializer = initializer;
            this.configurationOperator = configurationOperator;
            this.log = log;
        }

        /// <summary>
        /// Runs the specified test.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="configurationSubstitutions"></param>
        /// <param name="dataSource">The data source.</param>
        /// <returns></returns>
        public TestUnitResult Run(TestUnit test, DistributedConfigurationSubstitutions configurationSubstitutions, IAgentDataSource dataSource)
        {
            var project = projects.GetOrLoad(test.Run);
            if (project == null)
            {
                log.Info("Sending a request for project files...");
                PackedProject packedProject;
                try
                {
                    packedProject = dataSource.GetPackedProject(test.Run);
                    if (packedProject == null)
                        return new TestUnitResult(TestResultFactory.GetProjectRetrievalFailure(test));
                }
                catch(Exception ex)
                {
                    return new TestUnitResult(TestResultFactory.GetProjectRetrievalFailure(test, ex));
                }
                log.Info("Storing project...");
                project = projects.Store(test.Run, packedProject);
            }

            log.BeginActivity("Starting test execution...");
            var nUnitTestResult = GetNUnitTestResult(test, project, configurationSubstitutions);
            log.EndActivity("Test execution was finished");
            
            return new TestUnitResult(nUnitTestResult);
        }

        /// <summary>
        /// Gets the NUnit test result.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="project">The project.</param>
        /// <param name="configurationSubstitutions"></param>
        /// <returns></returns>
        public TestResult GetNUnitTestResult(TestUnit test, TestProject project, DistributedConfigurationSubstitutions configurationSubstitutions)
        {
            var nativeRunner = runnerCache.GetOrLoad(test.Run, configurationSubstitutions,
                                                     () =>
                                                         {
                                                             initializer.Initialize();

                                                             string configurationFileName =
                                                                 configurationOperator.GetSubstitutedConfigurationFile(project,
                                                                                                            test.Run.
                                                                                                                NUnitParameters,
                                                                                                            configurationSubstitutions);

                                                             var mappedAssemblyFile = Path.Combine(project.Path,
                                                                                                   Path.GetFileName(
                                                                                                       test.Run.NUnitParameters.
                                                                                                           AssembliesToTest[0]));
                                                             var package = new TestPackage(mappedAssemblyFile);
                                                             package.Settings["ShadowCopyFiles"] = true;
                                                             package.BasePath = package.PrivateBinPath = project.Path;
                                                             package.ConfigurationFile = configurationFileName 
                                                                 ?? Path.ChangeExtension(mappedAssemblyFile, ".config");
                                                             var nativeTestRunner = new MultipleTestDomainRunner();
                                                             nativeTestRunner.Load(package);
                                                             return nativeTestRunner;
                                                         });

            var testOptions = test.Run.NUnitParameters;
            TestResult testResult;
            
            try
            {
                testResult = nativeRunner.Run(new NullListener(),
                                              new NUnitTestsFilter(testOptions.IncludeCategories,
                                                                   testOptions.ExcludeCategories,
                                                                   test.UniqueTestId));
            }
            catch (Exception ex)
            {
                log.Error("Error while running test on agent", ex);
                throw;
            }
            return testResult;
        }
    }
}