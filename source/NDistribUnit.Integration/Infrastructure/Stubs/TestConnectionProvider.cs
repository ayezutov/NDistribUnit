using System.Linq;
using System.ServiceModel;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Integration.Tests.Infrastructure.Entities;

namespace NDistribUnit.Integration.Tests.Infrastructure.Stubs
{
    public class TestConnectionProvider: IConnectionProvider
    {
        private readonly NDistribUnitTestSystemController controller;
        private ITestRunnerClient lastClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestConnectionProvider"/> class.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public TestConnectionProvider(NDistribUnitTestSystemController controller)
        {
            this.controller = controller;
        }

        public TServiceContract GetCurrentCallback<TServiceContract>() where TServiceContract : class
        {
            if (typeof(TServiceContract) == typeof(ITestRunnerClient))
            {
                return (TServiceContract)lastClient;
            }
            return null;
        }

        public TServiceContract GetConnection<TServiceContract>(EndpointAddress address) where TServiceContract : class
        {
            if (typeof(TServiceContract) == typeof(ITestRunnerAgent))
            {
                AgentWrapper agentWrapper = controller.Agents
                    .FirstOrDefault(a => a.TestRunner.Name.Equals(address.Uri.AbsolutePath.Trim('/')));

                return (TServiceContract)(ITestRunnerAgent)(agentWrapper == null ? null : agentWrapper.TestRunner);
            }
            return null;
        }

        public TServiceContract GetDuplexConnection<TServiceContract, TCallbackType>(TCallbackType callback, EndpointAddress address) where TServiceContract : class where TCallbackType : class
        {
            if (typeof(TServiceContract) == typeof(ITestRunnerServer)
                && typeof(ITestRunnerClient).IsAssignableFrom(typeof(TCallbackType)))
            {
                lastClient = (ITestRunnerClient)callback;
                return (TServiceContract)(ITestRunnerServer)controller.Server.TestRunner;
            }

            if (typeof(TServiceContract) == typeof(ITestRunnerAgent) && typeof(TCallbackType) == typeof(ITestRunnerServer))
            {
                AgentWrapper agentWrapper = controller.Agents
                    .FirstOrDefault(a => a.TestRunner.Name.Equals(address.Uri.AbsolutePath.Trim('/')));

                return (TServiceContract)(ITestRunnerAgent)(agentWrapper == null ? null : agentWrapper.TestRunner);
            }

            return null;
        }
    }
}