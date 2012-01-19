using System;
using System.ServiceModel;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.DataContracts;

namespace NDistribUnit.Common.ServiceContracts
{
    /// <summary>
    /// A contract, which should be implemented by some side to be discoverable and trackable
    /// </summary>
    [ServiceContract(Namespace = ServiceConfiguration.Namespace)]
    public interface IPingable
    {
        /// <summary>
        /// Pings the tracking side
        /// </summary>
        /// <param name="pingInterval"></param>
        /// <returns>Anything (including null) if everything is ok, throws exception otherwise</returns>
        [OperationContract]
        PingResult Ping(TimeSpan pingInterval);
    }
}