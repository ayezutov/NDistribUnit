using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace NDistribUnit.Common.Communication
{
    public class DuplexClient<TIContract>: DuplexClientBase<TIContract> where TIContract : class
    {
        public DuplexClient(object callbackInstance) : base(callbackInstance)
        {
        }

        public DuplexClient(object callbackInstance, string endpointConfigurationName) : base(callbackInstance, endpointConfigurationName)
        {
        }

        public DuplexClient(object callbackInstance, string endpointConfigurationName, string remoteAddress) : base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        public DuplexClient(object callbackInstance, string endpointConfigurationName, EndpointAddress remoteAddress) : base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        public DuplexClient(object callbackInstance, Binding binding, EndpointAddress remoteAddress) : base(callbackInstance, binding, remoteAddress)
        {
        }

        public DuplexClient(InstanceContext callbackInstance) : base(callbackInstance)
        {
        }

        public DuplexClient(InstanceContext callbackInstance, string endpointConfigurationName) : base(callbackInstance, endpointConfigurationName)
        {
        }

        public DuplexClient(InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        public DuplexClient(InstanceContext callbackInstance, string endpointConfigurationName, EndpointAddress remoteAddress) : base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        public DuplexClient(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress) : base(callbackInstance, binding, remoteAddress)
        {
        }

        protected override TIContract CreateChannel()
        {
            TIContract channel = base.CreateChannel();
            return channel;
        }
    }
}