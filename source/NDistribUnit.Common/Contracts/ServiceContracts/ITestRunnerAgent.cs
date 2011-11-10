using System.ServiceModel;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.TestExecution;

namespace NDistribUnit.Common.Contracts.ServiceContracts
{
    /// <summary>
    /// A contract for communicating from server to agents
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IAgentDataSource))]
    public interface ITestRunnerAgent
    {
        /// <summary>
        /// Runs tests on agent
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        [OperationContract]
        TestResult RunTests(TestUnit test);
    }
}