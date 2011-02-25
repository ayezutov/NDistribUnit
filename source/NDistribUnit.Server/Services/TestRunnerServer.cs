using System;
using System.ServiceModel;
using System.Threading;
using NDistribUnit.Common;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Server.Services
{
    // NOTE: If you change the class name "Service1" here, you must also update the reference to "Service1" in App.config.
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class TestRunnerServer : ITestRunnerServer
    {
        private string g = Guid.NewGuid().ToString();


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


        public void RunTests()
        {
            throw new NotImplementedException();
        }
    }
}
