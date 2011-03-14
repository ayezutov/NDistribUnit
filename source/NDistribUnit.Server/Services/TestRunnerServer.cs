using System;
using System.ServiceModel;
using System.Threading;
using NDistribUnit.Common;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Server.Services
{
    /// <summary>
    /// The test runner service, which is used by clients to tell the job
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class TestRunnerServer : ITestRunnerServer
    {
        private string g = Guid.NewGuid().ToString();

        /// <summary>
        /// Method for testing purposes
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        // TODO: Should be finally deleted
        public string GetData(int value)
        {
            
            Console.WriteLine(string.Format("Guid: {0}", g));
            var callbacks = OperationContext.Current.GetCallbackChannel<ITestRunnerAgent>();
            new Timer(state =>
                          {
                              try
                              {
                                  Console.WriteLine("result: {0}", callbacks.RunTests(value.ToString()));
                              }
                              catch(Exception exce)
                              {
                                  Console.WriteLine("{0}: {1}", exce.GetType().FullName, exce.Message);
                              }
                          }).Change(100*1000, -1);
            return string.Format("You entered: {0}", value);
        }

        /// <summary>
        /// Runs tests from client
        /// </summary>
        public void RunTests()
        {
            throw new NotImplementedException();
        }
    }
}
