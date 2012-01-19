using System.ServiceModel;
using NDistribUnit.Common.Client;

namespace NDistribUnit.Common.Common.Communication
{
    /// <summary>
    /// 
    /// </summary>
    public interface IConnectionProvider
    {
        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <typeparam name="TServiceContract">The type of the service contract.</typeparam>
        TServiceContract GetConnection<TServiceContract>(EndpointAddress address) where TServiceContract: class;
    }
}