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
        /// <returns>true if everything is ok</returns>
        [OperationContract]
        bool Ping();
    }
}