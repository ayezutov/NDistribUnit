using System.ServiceModel;

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
        TServiceContract GetCurrentCallback<TServiceContract>();

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <typeparam name="TServiceContract">The type of the service contract.</typeparam>
        TServiceContract GetConnection<TServiceContract>(EndpointAddress address);
    }
}