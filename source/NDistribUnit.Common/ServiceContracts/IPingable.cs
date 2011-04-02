using System;
using System.ServiceModel;

namespace NDistribUnit.Common.ServiceContracts
{
    /// <summary>
    /// A contract, which should be implemented by some side to be discoverable and trackable
    /// </summary>
    [ServiceContract]
    public interface IPingable
    {
        /// <summary>
        /// Pings the tracking side
        /// </summary>
        /// <param name="pingInterval"></param>
        /// <returns>true if everything is ok</returns>
        [OperationContract]
        bool Ping(TimeSpan pingInterval);
    }
}