using System.ServiceModel;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Common.Logging;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Common.Contracts.ServiceContracts
{
    /// <summary>
    /// Interface, which represents standard operations with a remote
    /// part of a program
    /// </summary>
    [ServiceContract(Namespace = ServiceConfiguration.Namespace)]
    public interface IRemoteAppPart : IPingable
    {
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