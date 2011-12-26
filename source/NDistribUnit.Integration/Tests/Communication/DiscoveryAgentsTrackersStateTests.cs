using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.Server.AgentsTracking.AgentsProviders.Discovery;
using NDistribUnit.Integration.Tests.Infrastructure;
using NUnit.Framework;

namespace NDistribUnit.Integration.Tests.Tests.Communication
{
    [TestFixture, Explicit]
    public class DiscoveryAgentsTrackersStateTests : AgentsTrackersStateTestsBase
    {
        protected override void ConfigureSystem(NDistribUnitTestSystemFluent system)
        {
            system
                .ActAsRealSystemWithOpeningPorts()
                .Register(new DiscoveryAgentsProviderOptions{ DiscoveryIntervalInMiliseconds = 1000 })
                .SetConnectionsTracker<DiscoveryAgentsProvider>();
        }
    }
}
