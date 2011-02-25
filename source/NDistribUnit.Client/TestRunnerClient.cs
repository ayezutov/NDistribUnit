using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Client
{
    [System.Diagnostics.DebuggerStepThroughAttribute]
    public class TestRunnerClient : System.ServiceModel.DuplexClientBase<ITestRunnerServer>
    {

        public TestRunnerClient(System.ServiceModel.InstanceContext callbackInstance) :
            base(callbackInstance)
        {
        }

        public TestRunnerClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) :
            base(callbackInstance, endpointConfigurationName)
        {
        }

        public TestRunnerClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) :
            base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        public TestRunnerClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
            base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        public TestRunnerClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
            base(callbackInstance, binding, remoteAddress)
        {
        }

        public ITestRunnerServer Actions { get { return Channel; } }
    }
}