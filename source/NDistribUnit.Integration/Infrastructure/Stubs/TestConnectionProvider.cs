using System.Linq;
using System.ServiceModel;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Integration.Tests.Infrastructure.Entities;

namespace NDistribUnit.Integration.Tests.Infrastructure.Stubs
{
    public class TestConnectionProvider: IConnectionProvider
    {
        private readonly NDistribUnitTestSystemController controller;
        private IClient lastClient;

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
            if (typeof (TServiceContract) == typeof (IClient))
                return (TServiceContract) lastClient;
            
            return null;
        }

        public TServiceContract GetConnection<TServiceContract>(EndpointAddress address) where TServiceContract : class
        {
            if (typeof(TServiceContract) == typeof(IRemoteAppPart))
            {
                var name = address.Uri.AbsolutePath.Trim('/').Split('/')[0];
                AgentWrapper agentWrapper = controller.Agents
                    .FirstOrDefault(a => a.TestRunner.Name.Equals(name));

                return (TServiceContract)(IRemoteAppPart)(agentWrapper == null ? null : agentWrapper.TestRunner);
            }
            return null;
        }

        public TServiceContract GetDuplexConnection<TServiceContract, TCallbackType>(TCallbackType callback, EndpointAddress address) where TServiceContract : class where TCallbackType : class
        {
            if (typeof(TServiceContract) == typeof(IServer)
                && typeof(IClient).IsAssignableFrom(typeof(TCallbackType)))
            {
                lastClient = (IClient)callback;
                return (TServiceContract)(IServer)controller.Server.TestRunner;
            }

            if (typeof(TServiceContract) == typeof(IAgent) && typeof(TCallbackType) == typeof(IAgentDataSource))
            {
                AgentWrapper agentWrapper = controller.Agents
                    .FirstOrDefault(a => a.TestRunner.Name.Equals(address.Uri.AbsolutePath.Trim('/')));

                return (TServiceContract)(IAgent)(agentWrapper == null ? null : agentWrapper.TestRunner);
            }

            return null;
        }
    }
}