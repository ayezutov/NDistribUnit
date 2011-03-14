using System.ServiceModel;

namespace NDistribUnit.Common.ServiceContracts
{
    /// <summary>
    /// A contract for communicating from server to agents
    /// </summary>
    [ServiceContract]
    public interface ITestRunnerAgent: IPingable
    {
        /// <summary>
        /// Runs tests on agent
        /// </summary>
        /// <param name="callbackValue"></param>
        /// <returns></returns>
        [OperationContract]
        bool RunTests(string callbackValue);
    }
}