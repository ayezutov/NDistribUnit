using System.Linq;
using System.ServiceModel;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Integration.Tests.Infrastructure.Entities;

namespace NDistribUnit.Integration.Tests.Infrastructure.Stubs
{
    public class TestingConnectionProvider: IConnectionProvider
    {
        private readonly NDistribUnitTestSystem controller;
        private IClient lastClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestingConnectionProvider"/> class.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public TestingConnectionProvider(NDistribUnitTestSystem controller)
        {
            this.controller = controller;
        }

        public TServiceContract GetConnection<TServiceContract>(EndpointAddress address) where TServiceContract : class
        {
            if (typeof(TServiceContract) == typeof(IRemoteAppPart))
            {
                var name = address.Uri.LocalPath.Trim('/').Split('/')[0];
                AgentWrapper agentWrapper = controller.Agents
                    .FirstOrDefault(a => a.TestRunner.Name.Equals(name));

                return (TServiceContract)((IRemoteAppPart)agentWrapper ?? new CommunicationExceptionThrowingAgent());
            }

            if (typeof (TServiceContract) == typeof (IServer))
                return (TServiceContract) ((IServer) controller.Server.TestRunner);

            return null;
        }
    }
}