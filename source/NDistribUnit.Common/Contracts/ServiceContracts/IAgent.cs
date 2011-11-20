using System.Collections;
using System.ServiceModel;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.TestExecution;
using NUnit.Core;

namespace NDistribUnit.Common.Contracts.ServiceContracts
{
    /// <summary>
    /// A contract for communicating from server to agents
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IAgentDataSource))]
    [ServiceKnownType(typeof(ArrayList))]
    [ServiceKnownType(typeof(TestResult))]
    public interface IAgent
    {
        /// <summary>
        /// Runs tests on agent
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        [OperationContract]
        TestUnitResult RunTests(TestUnit test);
    }
}