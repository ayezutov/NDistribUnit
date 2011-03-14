using System.ServiceModel;

namespace NDistribUnit.Common.ServiceContracts
{
    /// <summary>
    /// Contract, which is used by client to connect to server
    /// </summary>
    [ServiceContract]
    public interface ITestRunnerServer
    {
        /// <summary>
        /// Runs tests from client
        /// </summary>
        [OperationContract]
        void RunTests();
    }
}