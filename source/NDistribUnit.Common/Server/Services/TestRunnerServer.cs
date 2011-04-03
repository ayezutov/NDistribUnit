using System;
using System.ServiceModel;
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
        /// Runs tests from client
        /// </summary>
        public void RunTests()
        {
            throw new NotImplementedException();
        }
    }
}
