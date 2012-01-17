using System;
using System.IO;
using System.Threading;
using NDistribUnit.Common.Common.ConsoleProcessing;
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
        private readonly ExceptionCatcher exceptionCatcher;
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTestRunner"/> class.
        /// </summary>
        /// <param name="projects">The storage.</param>
        /// <param name="runnerCache">The cash.</param>
        /// <param name="initializer">The initializer.</param>
        /// <param name="configurationOperator">The configuration operator.</param>
        /// <param name="exceptionCatcher">The exception catcher.</param>
        /// <param name="log">The log.</param>
        public AgentTestRunner(
            IProjectsStorage projects,
            INativeRunnerCache runnerCache,
            ITestSystemInitializer initializer,
            IDistributedConfigurationOperator configurationOperator,
            ExceptionCatcher exceptionCatcher,
            ILog log)
        {
            this.projects = projects;
            this.runnerCache = runnerCache;
            this.initializer = initializer;
            this.configurationOperator = configurationOperator;
            this.exceptionCatcher = exceptionCatcher;
            this.log = log;
        }

        /// <summary>
        /// Runs the specified test.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="configurationSubstitutions"></param>
        /// <returns></returns>
        public TestResult Run(TestUnit test, DistributedConfigurationSubstitutions configurationSubstitutions)
        {
            var project = projects.Get(test.Run);
            if (project == null)
            {
                log.Info("Project file was not found. Throwing exception");
                return TestResultFactory.GetProjectRetrievalFailure(test);
            }

            log.BeginActivity("Starting test execution...");
            var nUnitTestResult = GetNUnitTestResult(test, project, configurationSubstitutions);
            log.EndActivity("Test execution was finished");
            
            return nUnitTestResult;
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

                                                             TestPackage package;
                                                             if (!NUnitProject.IsNUnitProjectFile(mappedAssemblyFile))
                                                                 package = new TestPackage(mappedAssemblyFile);
                                                             else
                                                             {
                                                                 var nunitProject = new NUnitProject(mappedAssemblyFile);
                                                                 nunitProject.Load();
                                                                 if (!string.IsNullOrEmpty(test.Run.NUnitParameters.Configuration))
                                                                     nunitProject.SetActiveConfig(test.Run.NUnitParameters.Configuration);

                                                                 package = nunitProject.ActiveConfig.MakeTestPackage();
                                                             }
                                                             
                                                             package.Settings["ShadowCopyFiles"] = true;
                                                             package.BasePath = package.PrivateBinPath = project.Path;

                                                             if (!string.IsNullOrEmpty(configurationFileName))
                                                             {
                                                                 package.ConfigurationFile = configurationFileName;
                                                                     /*?? Path.ChangeExtension(mappedAssemblyFile, ".config");*/
                                                             }

                                                             var nativeTestRunner = new MultipleTestDomainRunner();
                                                             nativeTestRunner.Load(package);
                                                             return nativeTestRunner;
                                                         });

            var testOptions = test.Run.NUnitParameters;
            TestResult testResult = null;
            
            try
            {
                Action runTest = ()=>
                                     {
                                         testResult = nativeRunner.Run(new NullListener(),
                                                                       new NUnitTestsFilter(
                                                                           testOptions.IncludeCategories,
                                                                           testOptions.ExcludeCategories,
                                                                           test.UniqueTestId));
                                     };

                if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA
                    && !Thread.CurrentThread.TrySetApartmentState(ApartmentState.STA))
                {
                    var thread = new Thread(()=> exceptionCatcher.Run(runTest));
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    thread.Join();
                }
                else
                {
                    runTest();
                }
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