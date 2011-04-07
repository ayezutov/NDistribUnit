using System.ServiceModel;
using NDistribUnit.Common.Logging;

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

        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <param name="maxItemsCount">The max items count.</param>
        /// <param name="lastFetchedEntryId">The last fetched entry id.</param>
        /// <returns></returns>
        [OperationContract]
        LogEntry[] GetLog(int maxItemsCount, int? lastFetchedEntryId);
    }
}