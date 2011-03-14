using System;
using System.ServiceModel;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Agent.Communication
{
    /// <summary>
    /// The service, which is communicated, when the server calls he agent
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class TestRunnerAgent : ITestRunnerAgent
    {
        /// <summary>
        /// Runs tests on agent
        /// </summary>
        /// <param name="callbackValue"></param>
        /// <returns></returns>
        public bool RunTests(string callbackValue)
        {
            System.Console.WriteLine("Callback Received: {0}", callbackValue);
            return true;
        }


        /// <summary>
        /// Pings the tracking side
        /// </summary>
        /// <returns>true if everything is ok</returns>
        public bool Ping()
        {
            return true;
        }
    }
}