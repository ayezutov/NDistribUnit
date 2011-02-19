using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using NDistribUnit.Common;

namespace NDistribUnit.Server
{
    // NOTE: If you change the class name "Service1" here, you must also update the reference to "Service1" in App.config.
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class TestRunner : ITestRunner
    {
        private string g = Guid.NewGuid().ToString();


        public string GetData(int value)
        {
            Console.WriteLine(string.Format("Guid: {0}", g));
            var callbacks = OperationContext.Current.GetCallbackChannel<ICallbacks>();
            new Timer(state =>
                          {
                              try
                              {
                                  Console.WriteLine("result: {0}", callbacks.MyCallbackFunction(value.ToString()));
                              }
                              catch(Exception exce)
                              {
                                  Console.WriteLine("{0}: {1}", exce.GetType().FullName, exce.Message);
                              }
                          }).Change(100*1000, -1);
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }

    }
}
