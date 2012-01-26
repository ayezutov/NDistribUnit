using System;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.TestExecution.Configuration;
using NDistribUnit.Common.TestExecution.Storage;
using NUnit.Core;
using NUnit.Util;

namespace NDistribUnit.Common.Client
{
    /// <summary>
    /// 
    /// </summary>
    public class Client : IClient
    {
        private readonly ClientParameters options;
        private readonly IUpdateReceiver updateReceiver;
        private readonly IConnectionProvider connectionProvider;
        private readonly IVersionProvider versionProvider;
        private readonly ITestRunParametersFileReader parametersReader;
        private readonly IProjectPackager packager;
        private readonly ITestResultsSerializer serializer;
        private readonly ILog log;
        private TestResult result;
        private TestRun testRun;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="updateReceiver">The update receiver.</param>
        /// <param name="connectionProvider">The connection provider.</param>
        /// <param name="versionProvider">The version provider.</param>
        /// <param name="parametersReader">The file reader.</param>
        /// <param name="packager">The packager.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="log">The log.</param>
        public Client(ClientParameters options,
                      IUpdateReceiver updateReceiver,
                      IConnectionProvider connectionProvider,
                      IVersionProvider versionProvider,
                      ITestRunParametersFileReader parametersReader,
                      IProjectPackager packager,
                      ITestResultsSerializer serializer,
                      ILog log)
        {
            this.options = options;
            this.updateReceiver = updateReceiver;
            this.connectionProvider = connectionProvider;
            this.versionProvider = versionProvider;
            this.parametersReader = parametersReader;
            this.packager = packager;
            this.serializer = serializer;
            this.log = log;
        }

        /// <summary>
        /// Pings the tracking side
        /// </summary>
        /// <param name="pingInterval"></param>
        /// <returns>
        /// Anything (including null) if everything is ok, throws exception otherwise
        /// </returns>
        public PingResult Ping(TimeSpan pingInterval)
        {
            return null;
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        public int Run()
        {
            var server = connectionProvider.GetConnection<IServer>(new EndpointAddress(options.ServerUri));

            testRun = new TestRun
                          {
                              NUnitParameters = options.NUnitParameters,
                              Alias = options.Alias
                          };
            //TODO: load saved state here

            log.Info(string.Format("Test run was given the following unqiue identifier: '{0}'", testRun.Id));

            if (options.NUnitParameters.AssembliesToTest == null || options.NUnitParameters.AssembliesToTest.Count != 1
                /*|| !NUnitProject.IsNUnitProjectFile(options.AssembliesToTest[0])*/)
                throw new InvalidOperationException("Please specify single NUnit project file");

            string parametersFileName = Path.ChangeExtension(options.NUnitParameters.AssembliesToTest[0],
                                                             ".ndistribunit");

            testRun.Parameters = File.Exists(parametersFileName)
                                     ? parametersReader.Read(parametersFileName)
                                     : TestRunParameters.Default;

            try
            {
                log.BeginActivity("Checking project existence on server...");
                bool hasProject = server.HasProject(testRun);
                if (!hasProject)
                {
                    log.EndActivity("Project is absent on server");

                    log.BeginActivity("Packaging project...");
                    Stream packageStream = packager.GetPackage(testRun.NUnitParameters.AssembliesToTest);

                    log.EndActivity("Project packaging completed.");

                    var fs = packageStream as FileStream;
                    if (fs != null)
                        log.Info(string.Format("Stream '{0}': {1}", fs.Name, fs.Length));

                    try
                    {
                        log.BeginActivity("Sending project to server...");
                        server.ReceiveProject(new ProjectMessage
                                                  {
                                                      Project = packageStream,
                                                      TestRun = testRun
                                                  });
                        log.EndActivity("Project was successfully sent to server");
                    }
                    finally
                    {
                        packageStream.Close();
                    }
                }
                else
                {
                    log.EndActivity("Project is already on server");
                }
            }
            catch (EndpointNotFoundException)
            {
                log.Error("It seems, that the server is not available");
                //TODO: Save a failed test run here
                return (int) ReturnCodes.ServerNotAvailable;
            }
            catch (CommunicationException ex)
            {
                log.Error("There was an error, when trying to send the package to client", ex);
                //TODO: Save a failed test run here
                return (int) ReturnCodes.NetworkConnectivityError;
            }

            var testRunningTask = Task.Factory.StartNew(() =>
                                                            {
                                                                try
                                                                {
                                                                    log.BeginActivity("Started running tests...");
                                                                    TestResult tempResult;
                                                                    while ((tempResult = server.RunTests(testRun)) !=
                                                                           null)
                                                                    {
                                                                        if (tempResult.IsFinal())
                                                                        {
                                                                            result = tempResult;
                                                                            log.Info("Running all tests completed!");
                                                                            break;
                                                                        }

                                                                        PrintSummaryInfoForResult(tempResult,
                                                                                                  new ResultSummarizer(
                                                                                                      tempResult));
                                                                    }
                                                                    log.EndActivity("Finished running tests");
                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    log.Error("An error occurred while running tests",
                                                                              ex);
                                                                }
                                                            });
            var updateTask = Task.Factory.StartNew(() =>
                                                       {
                                                           try
                                                           {
                                                               log.BeginActivity("Checking for updates...");
                                                               var updatePackage =
                                                                   server.GetUpdatePackage(new UpdateRequest
                                                                                               {
                                                                                                   Version =
                                                                                                       versionProvider.
                                                                                                       GetVersion()
                                                                                               });
                                                               if (updatePackage.IsAvailable)
                                                               {
                                                                   log.EndActivity("Update package available");

                                                                   log.BeginActivity(
                                                                       string.Format("Receiving update to {0}...",
                                                                                     updatePackage.Version));
                                                                   updateReceiver.SaveUpdatePackage(updatePackage);
                                                                   log.EndActivity(
                                                                       string.Format(
                                                                           "Update {0} was successfully received",
                                                                           updatePackage.Version));
                                                               }
                                                               else
                                                                   log.EndActivity("No updates available.");
                                                           }
                                                           catch (Exception ex)
                                                           {
                                                               log.Warning(
                                                                   "There was an exception when trying to get an update",
                                                                   ex);
                                                           }
                                                       });
            try
            {
                Task.WaitAll(new[] {testRunningTask, updateTask}, options.TimeoutPeriod);
            }
            catch (AggregateException ex)
            {
                foreach (var innerException in ex.InnerExceptions)
                {
                    log.Error("Error:", innerException);
                }
                throw;
            }
            catch (Exception ex)
            {
                log.Error("Error while running tests", ex);
                throw;
            }

            SaveResult();

            if (result == null)
            {
                log.Info("Result is not available. Maybe not all tests were run?");
                return (int) ReturnCodes.NoTestsAvailable;
            }

            var summary = new ResultSummarizer(result);
            PrintSummaryInfoForResult(result, summary);
            return summary.ErrorsAndFailures;
        }

        private void PrintSummaryInfoForResult(TestResult testResult, ResultSummarizer summary)
        {
            log.Info(string.Format("Tests run: {0}, Errors: {1}, Failures: {2}, Inconclusive: {3}, Time: {4} seconds",
                                   summary.TestsRun, summary.Errors, summary.Failures, summary.Inconclusive,
                                   summary.Time));
            log.Info(string.Format("  Not run: {0}, Invalid: {1}, Ignored: {2}, Skipped: {3}",
                                   summary.TestsNotRun, summary.NotRunnable, summary.Ignored, summary.Skipped));

            if (summary.ErrorsAndFailures > 0 || testResult.IsError || testResult.IsFailure)
                WriteErrorsAndFailures(testResult);
        }

        private void WriteErrorsAndFailures(TestResult testResult)
        {
            if (testResult.Executed)
            {
                if (testResult.HasResults)
                {
                    if (testResult.IsFailure || testResult.IsError)
                        if (testResult.FailureSite == FailureSite.SetUp ||
                            testResult.FailureSite == FailureSite.TearDown)
                            WriteSingleResult(testResult);

                    foreach (TestResult childResult in testResult.Results)
                        WriteErrorsAndFailures(childResult);
                }
                else if (testResult.IsFailure || testResult.IsError)
                {
                    WriteSingleResult(testResult);
                }
            }
        }

        private void WriteSingleResult(TestResult testResult)
        {
            string status = testResult.IsFailure || testResult.IsError
                                ? string.Format("{0} {1}", testResult.FailureSite, testResult.ResultState)
                                : testResult.ResultState.ToString();

            log.Info(string.Format("{0} : {1}", status, testResult.FullName));

            if (!string.IsNullOrEmpty(testResult.Message))
                log.Info(string.Format("   {0}", testResult.Message));

            if (!string.IsNullOrEmpty(testResult.StackTrace))
                log.Info(testResult.IsFailure
                             ? StackTraceFilter.Filter(testResult.StackTrace)
                             : testResult.StackTrace + Environment.NewLine);
        }

        private void SaveResult()
        {
            if (result == null)
                return;

            if (!string.IsNullOrEmpty(options.NUnitParameters.XmlFileName))
            {
                log.BeginActivity(string.Format("Saving results to '{0}'...", options.NUnitParameters.XmlFileName));

                var xml = serializer.GetXml(result);
                var writer = new StreamWriter(options.NUnitParameters.XmlFileName);
                try
                {
                    writer.Write(xml);
                }
                finally
                {
                    writer.Close();
                }

                log.EndActivity("Results were saved");
            }
        }
    }
}