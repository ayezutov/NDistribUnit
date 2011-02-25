using System;
using System.ServiceModel;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Agent.Communication
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class TestRunnerAgent : ITestRunnerAgent
    {
        public bool RunTests(string callbackValue)
        {
            System.Console.WriteLine("Callback Received: {0}", callbackValue);
            return true;
        }

        public bool Ping()
        {
            return true;
        }
    }
}