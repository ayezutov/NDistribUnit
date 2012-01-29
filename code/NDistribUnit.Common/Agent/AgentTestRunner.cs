using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Threading;
using NDistribUnit.Common.Common.ConsoleProcessing;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.TestExecution.DistributedConfiguration;
using NDistribUnit.Common.TestExecution.Storage;
using NDistribUnit.Common.Updating;
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
        private readonly BootstrapperParameters bootstrapperParameters;
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTestRunner"/> class.
        /// </summary>
        /// <param name="projects">The storage.</param>
        /// <param name="runnerCache">The cash.</param>
        /// <param name="initializer">The initializer.</param>
        /// <param name="configurationOperator">The configuration operator.</param>
        /// <param name="exceptionCatcher">The exception catcher.</param>
        /// <param name="bootstrapperParameters">The bootstrapper parameters.</param>
        /// <param name="log">The log.</param>
        public AgentTestRunner(
            IProjectsStorage projects,
            INativeRunnerCache runnerCache,
            ITestSystemInitializer initializer,
            IDistributedConfigurationOperator configurationOperator,
            ExceptionCatcher exceptionCatcher,
            BootstrapperParameters bootstrapperParameters,
            ILog log)
        {
            this.projects = projects;
            this.runnerCache = runnerCache;
            this.initializer = initializer;
            this.configurationOperator = configurationOperator;
            this.exceptionCatcher = exceptionCatcher;
            this.bootstrapperParameters = bootstrapperParameters;
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
        /// <param name="configurationSubstitutions">The configuration substitutions.</param>
        /// <param name="isChild">if set to <c>true</c> [is child].</param>
        /// <returns></returns>
        public TestResult GetNUnitTestResult(TestUnit test, TestProject project, DistributedConfigurationSubstitutions configurationSubstitutions, bool isChild = false)
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
                                                             package.AutoBinPath = true;
//                                                             package.BasePath = bootstrapperParameters.RootFolder;
//                                                             package.PrivateBinPath =
//                                                                 DomainManager.GetPrivateBinPath(
//                                                                     bootstrapperParameters.RootFolder,
//                                                                     new ArrayList()
//                                                                         {
//                                                                             Assembly.GetExecutingAssembly().Location,
//                                                                             mappedAssemblyFile
//                                                                         });
                                                             if (!string.IsNullOrEmpty(configurationFileName))
                                                             {
                                                                 package.ConfigurationFile = configurationFileName;
                                                             }

                                                             var nativeTestRunner = new DefaultTestRunnerFactory().MakeTestRunner(package);
                                                             nativeTestRunner.Load(package);
                                                             return nativeTestRunner;
                                                         });

            var testOptions = test.Run.NUnitParameters;
            TestResult testResult = null;
            
            try
            {
                Action runTest = ()=>
                                     {
                                         try
                                         {
                                             testResult = nativeRunner.Run(new NullListener(),
                                                                           new NUnitTestsFilter(
                                                                               testOptions.IncludeCategories,
                                                                               testOptions.ExcludeCategories,
                                                                               test.UniqueTestId).NativeFilter);
                                         }
                                         //TODO: remove this. This is for tracking purposes only
                                         catch(AppDomainUnloadedException ex)
                                         {
                                             log.Warning("AppDomainUnloadedException is still being thrown", ex);
                                             if (!isChild)
                                             {
                                                 runnerCache.Remove(test.Run, configurationSubstitutions);
                                                 testResult = GetNUnitTestResult(test, project,
                                                                                 configurationSubstitutions,
                                                                                 isChild: true);
                                             }
                                             else
                                                 throw;
                                         }
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

        /// <summary>
        /// Unloads the specified test run.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        public void Unload(TestRun testRun)
        {
            runnerCache.Remove(testRun);
        }
    }
}