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
        /// Gets the current callback.
        /// </summary>
        /// <typeparam name="TServiceContract">The type of the service contract.</typeparam>
        TServiceContract GetCurrentCallback<TServiceContract>() where TServiceContract : class;

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <typeparam name="TServiceContract">The type of the service contract.</typeparam>
        TServiceContract GetConnection<TServiceContract>(EndpointAddress address) where TServiceContract: class;

        /// <summary>
        /// Gets the duplex connection.
        /// </summary>
        /// <typeparam name="TServiceContract">The type of the service contract.</typeparam>
        /// <typeparam name="TCallbackType">The type of the callback type.</typeparam>
        /// <param name="callback">The callback.</param>
        /// <param name="address"></param>
        TServiceContract GetDuplexConnection<TServiceContract, TCallbackType>(TCallbackType callback, EndpointAddress address) 
            where TServiceContract : class
            where TCallbackType: class;
    }
}