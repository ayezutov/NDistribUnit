using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.Server.ConnectionTracking.Discovery;
using NDistribUnit.Integration.Tests.Infrastructure;
using NUnit.Framework;

namespace NDistribUnit.Integration.Tests.Tests.Communication
{
    [TestFixture, Explicit]
    public class DiscoveryConnectionTrackersStateTests : ConnectionTrackersStateTestsBase
    {
        protected override void ConfigureSystem(NDistribUnitTestSystem system)
        {
            system
                .ActAsRealSystemWithOpeningPorts()
                .Register(new DiscoveryConnectionTrackerOptions{ DiscoveryIntervalInMiliseconds = 1000 })
                .SetConnectionsTracker<DiscoveryConnectionTracker<IRemoteParticle>>();
        }
    }
}
