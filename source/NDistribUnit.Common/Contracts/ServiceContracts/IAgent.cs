using System.Collections;
using System.ServiceModel;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.TestExecution.DistributedConfiguration;
using NUnit.Core;

namespace NDistribUnit.Common.Contracts.ServiceContracts
{
    /// <summary>
    /// A contract for communicating from server to agents
    /// </summary>
    [ServiceKnownType(typeof(ArrayList))]
    [ServiceKnownType(typeof(TestResult))]
    [ServiceContract(Namespace = "http://yezutov.com/ndistribunit")]
    public interface IAgent : IProjectReceiver
    {
        /// <summary>
        /// Runs tests on agent
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="configurationSubstitutions">The configuration substitutions.</param>
        /// <returns></returns>
        [OperationContract]
        TestUnitResult RunTests(TestUnit test, DistributedConfigurationSubstitutions configurationSubstitutions);
    }
}