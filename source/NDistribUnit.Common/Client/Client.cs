using System;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.TestExecution.Configuration;
using NDistribUnit.Common.TestExecution.Storage;
using NUnit.Core;
using System.Linq;

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
        public void Run()
        {
            var server = connectionProvider.GetConnection<IServer>(new EndpointAddress(options.ServerUri));

            testRun = new TestRun
                          {
                              NUnitParameters = options.NUnitParameters,
                              Alias = options.Alias
                          }; //TODO: load saved state here

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
                bool hasProject = server.HasProject(testRun);
                if (!hasProject)
                {
                    Stream packageStream = packager.GetPackage(testRun.NUnitParameters.AssembliesToTest);
                    try
                    {
                        server.ReceiveProject(new ProjectMessage
                                                  {
                                                      Project = new StreamWrapper(packageStream, log),
                                                      TestRun = testRun
                                                  });
                    }
                    finally
                    {
                        packageStream.Close();
                    }
                }
            }
            catch (EndpointNotFoundException)
            {
                log.Error("It seems, that the server is not available");
                //TODO: Save a failed test run here
                return;
            }
            catch(CommunicationException ex)
            {
                log.Error("There was an error, when trying to send the package to client", ex);
                //TODO: Save a failed test run here
                return;
            }

            var testRunningTask = Task.Factory.StartNew(() =>
                                                            {
                                                                try
                                                                {
                                                                    TestResult tempResult;
                                                                    while ((tempResult = server.RunTests(testRun)) != null)
                                                                    {
                                                                        if (tempResult.IsFinal())
                                                                        {
                                                                            result = tempResult;
                                                                            log.Info("Running all tests completed!");
                                                                            break;
                                                                        }

                                                                        log.Info(
                                                                            string.Format(
                                                                                "Test '{0}' completed: {1} run, {2} successful, {3} failed",
                                                                                tempResult.FullName,
                                                                                tempResult.FindDescedants(
                                                                                    d => !d.Test.IsSuite).Count(),
                                                                                tempResult.FindDescedants(
                                                                                    d => !d.Test.IsSuite && d.IsSuccess).Count(),
                                                                                tempResult.FindDescedants(
                                                                                    d => !d.Test.IsSuite && d.IsFailure).Count()));
                                                                    }
                                                                }
                                                                catch(Exception ex)
                                                                {
                                                                    log.Error("An error occurred while running tests", ex);
                                                                }
                                                            });
            var updateTask = Task.Factory.StartNew(() =>
                                          {
                                              try
                                              {
                                                  var updatePackage =
                                                      server.GetUpdatePackage(new UpdateRequest {Version = versionProvider.GetVersion()});
                                                  if (updatePackage.IsAvailable)
                                                      updateReceiver.SaveUpdatePackage(updatePackage);
                                              }
                                              catch (Exception ex)
                                              {
                                                  log.Warning("There was an exception when trying to get an update", ex);
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
            PrintResult();
        }

        private void PrintResult()
        {
            if (result == null)
            {
                log.Info("Result is not available. Maybe not all tests were run?");
                return;
            }

            var total = 0;
            var success = 0;
            result.ForSelfAndAllDescedants(r =>
                                               {
                                                   if (!r.Test.IsSuite)
                                                   {
                                                       total++;
                                                       if (r.IsSuccess)
                                                           success++;
                                                   }
                                               });
            log.Info(string.Format("{0} test cases were run. {1} of them completed successfully.", total, success));
        }

        private void SaveResult()
        {
            if (result == null)
                return;

            if (!string.IsNullOrEmpty(options.NUnitParameters.XmlFileName))
            {
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

            }
        }
    }
}