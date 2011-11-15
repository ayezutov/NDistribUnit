using System;
using System.IO;
using System.ServiceModel;
using System.Threading;
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
        private readonly ILog log;
        private TestResult result;
        private readonly Semaphore testCompleted;
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
        /// <param name="log">The log.</param>
        public Client(ClientParameters options,
                                IUpdateReceiver updateReceiver,
                                IConnectionProvider connectionProvider,
                                IVersionProvider versionProvider,
                                ITestRunParametersFileReader parametersReader,
                                IProjectPackager packager,
                                ILog log)
        {
            this.options = options;
            this.updateReceiver = updateReceiver;
            this.connectionProvider = connectionProvider;
            this.versionProvider = versionProvider;
            this.parametersReader = parametersReader;
            this.packager = packager;
            this.log = log;
            testCompleted = new Semaphore(0,1);
        }

        /// <summary>
        /// Notifies that the test has completed.
        /// </summary>
        /// <param name="receivedResult">The result.</param>
        /// <param name="isCompleted"></param>
        public void NotifyTestProgressChanged(TestResult receivedResult, bool isCompleted)
        {
            result = receivedResult;
            if (isCompleted)
                testCompleted.Release();
        }

        /// <summary>
        /// Gets the project.
        /// </summary>
        /// <param name="testRunId">The test run id.</param>
        /// <returns></returns>
        public PackedProject GetPackedProject(Guid testRunId)
        {
            if (testRun == null)
                throw new InvalidOperationException("Can't load any project from that client, as it is not initialized yet");

            if (testRun.Id != testRunId)
                throw new ArgumentException("The identifier should be of the client, which issued the test request", "testRunId");

            return new PackedProject(packager.GetPackage(testRun.NUnitParameters.AssembliesToTest));
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
            var server = connectionProvider.GetDuplexConnection<IServer, IClient>(
                this, new EndpointAddress(options.ServerUri));

            testRun = new TestRun
                          {
                              NUnitParameters = options.NUnitParameters,
                              Alias = options.Alias
                          }; //TODO: load saved state here

            if (options.NUnitParameters.AssembliesToTest == null || options.NUnitParameters.AssembliesToTest.Count != 1 /*|| !NUnitProject.IsNUnitProjectFile(options.AssembliesToTest[0])*/)
                throw new InvalidOperationException("Please specify single NUnit project file");

            string parametersFileName = Path.ChangeExtension(options.NUnitParameters.AssembliesToTest[0], ".ndistribunit");

            testRun.Parameters = File.Exists(parametersFileName)
                                 ? parametersReader.Read(parametersFileName)
                                 : TestRunParameters.Default;

            
            var testRunningTask = Task.Factory.StartNew(() =>
                                                            {
                                                                server.StartRunningTests(testRun);
                                                                testCompleted.WaitOne();
                                                                // TODO: show results and/or write to xml file
                                                            });

            var updateTask = Task.Factory.StartNew(() =>
                                                       {
                                                           var updatePackage =
                                                               server.GetUpdatePackage(versionProvider.GetVersion());
                                                           if (updatePackage.IsAvailable)
                                                            updateReceiver.SaveUpdatePackage(updatePackage);
                                                       });
            try
            {
                Task.WaitAll(new []{testRunningTask, updateTask}, options.TimeoutPeriod);
            }
            catch (AggregateException ex)
            {
                foreach (var innerException in ex.InnerExceptions)
                {
                    log.Error("Error:", innerException);
                }
                throw;
            }
            catch(Exception ex)
            {
                log.Error("Error while running tests", ex);
                throw;
            }
            SaveResult();
            PrintResult();
        }

        private void PrintResult()
        {
            //throw new NotImplementedException();
        }

        private void SaveResult()
        {
            //throw new NotImplementedException();
        }
    }
}