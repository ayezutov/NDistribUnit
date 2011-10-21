using System.ServiceModel;
using NDistribUnit.Common.Common.Communication;

namespace NDistribUnit.Common.Common.Networking
{
    /// <summary>
    /// 
    /// </summary>
    public class RealConnectionProvider : IConnectionProvider
    {
        /// <summary>
        /// Gets the current callback.
        /// </summary>
        /// <typeparam name="TServiceContract">The type of the service contract.</typeparam>
        /// <returns></returns>
        public TServiceContract GetCurrentCallback<TServiceContract>()
        {
            return OperationContext.Current.GetCallbackChannel<TServiceContract>();
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <typeparam name="TServiceContract">The type of the service contract.</typeparam>
        /// <param name="address"></param>
        /// <returns></returns>
        public TServiceContract GetConnection<TServiceContract>(EndpointAddress address)
        {
            return ChannelFactory<TServiceContract>.CreateChannel(new NetTcpBinding("NDistribUnit.Default"), address);
        }
    }
}