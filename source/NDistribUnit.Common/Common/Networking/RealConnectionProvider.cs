using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
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
        public TServiceContract GetCurrentCallback<TServiceContract>() where TServiceContract : class
        {
            return OperationContext.Current.GetCallbackChannel<TServiceContract>();
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <typeparam name="TServiceContract">The type of the service contract.</typeparam>
        /// <param name="address"></param>
        /// <returns></returns>
        public TServiceContract GetConnection<TServiceContract>(EndpointAddress address) where TServiceContract : class
        {
            return ChannelFactory<TServiceContract>.CreateChannel(new NetTcpBinding("NDistribUnit.Default"), address);
        }

        /// <summary>
        /// Gets the duplex connection.
        /// </summary>
        /// <typeparam name="TServiceContract">The type of the service contract.</typeparam>
        /// <typeparam name="TCallbackType">The type of the callback type.</typeparam>
        /// <param name="callback">The callback.</param>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        public TServiceContract GetDuplexConnection<TServiceContract, TCallbackType>(TCallbackType callback, EndpointAddress address) where TServiceContract : class where TCallbackType : class
        {
            var factory = new DuplexChannelFactory<TServiceContract>(callback, new NetTcpBinding("NDistribUnit.Default"), address);
            foreach (OperationDescription op in factory.Endpoint.Contract.Operations)
            {
                var dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
                if (dataContractBehavior != null)
                    dataContractBehavior.MaxItemsInObjectGraph = 2147483647;
            }
            return factory.CreateChannel();
        }
    }
}