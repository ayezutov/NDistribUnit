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
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Common.TestExecution.Configuration;

namespace NDistribUnit.Common.Client
{
    /// <summary>
    /// 
    /// </summary>
    public class TestRunnerClient : ITestRunnerClient
    {
        private readonly ClientParameters options;
        private readonly IUpdateReceiver updateReceiver;
        private readonly IConnectionProvider connectionProvider;
        private readonly IVersionProvider versionProvider;
        private readonly ITestRunParametersFileReader parametersReader;
        private readonly ILog log;
        private TestResult result;
        private Semaphore testCompleted;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunnerClient"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="updateReceiver">The update receiver.</param>
        /// <param name="connectionProvider">The connection provider.</param>
        /// <param name="versionProvider">The version provider.</param>
        /// <param name="parametersReader">The file reader.</param>
        /// <param name="log">The log.</param>
        public TestRunnerClient(ClientParameters options,
                                IUpdateReceiver updateReceiver,
                                IConnectionProvider connectionProvider,
                                IVersionProvider versionProvider,
                                ITestRunParametersFileReader parametersReader,
                                ILog log)
        {
            this.options = options;
            this.updateReceiver = updateReceiver;
            this.connectionProvider = connectionProvider;
            this.versionProvider = versionProvider;
            this.parametersReader = parametersReader;
            this.log = log;
            testCompleted = new Semaphore(0,1);
        }

        /// <summary>
        /// Notifies that the test has completed.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="isCompleted"></param>
        public void NotifyTestProgressChanged(TestResult result, bool isCompleted)
        {
            this.result = result;
            if (isCompleted)
                testCompleted.Release();
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
            var server = connectionProvider.GetDuplexConnection<ITestRunnerServer, TestRunnerClient>(
                this, new EndpointAddress(options.ServerUri));

            var run = new TestRun(); //TODO: load saved state here

            if (options.AssembliesToTest == null || options.AssembliesToTest.Count != 1 || Path.GetExtension(options.AssembliesToTest[0]) != ".nunit")
                throw new InvalidOperationException("Please specify single NUnit project file");

            string parametersFileName = Path.ChangeExtension(options.AssembliesToTest[0], ".ndistribunit");

            run.Parameters = File.Exists(parametersFileName)
                                 ? parametersReader.Read(parametersFileName)
                                 : TestRunParameters.Default;

            
            var testRunningTask = Task.Factory.StartNew(() =>
                                                            {
                                                                server.StartRunningTests(run);
                                                                testCompleted.WaitOne();
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