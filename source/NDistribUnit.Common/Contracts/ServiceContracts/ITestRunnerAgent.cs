using System.ServiceModel;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Common.TestExecution;

namespace NDistribUnit.Common.Contracts.ServiceContracts
{
    /// <summary>
    /// A contract for communicating from server to agents
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IAgentDataSource))]
    public interface ITestRunnerAgent: IPingable
    {
        /// <summary>
        /// Runs tests on agent
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        [OperationContract]
        TestResult RunTests(TestUnit test);

        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <param name="maxItemsCount">The max items count.</param>
        /// <param name="lastFetchedEntryId">The last fetched entry id.</param>
        /// <returns></returns>
        [OperationContract]
        LogEntry[] GetLog(int maxItemsCount, int? lastFetchedEntryId);

    	/// <summary>
    	/// Receives the update pakage.
    	/// </summary>
    	/// <param name="updatePackage"></param>
    	[OperationContract]
    	void ReceiveUpdatePackage(UpdatePackage updatePackage);
    }
}