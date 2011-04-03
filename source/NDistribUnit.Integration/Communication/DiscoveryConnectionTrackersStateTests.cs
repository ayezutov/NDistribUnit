using NDistribUnit.Common.Communication.ConnectionTracking.Discovery;
using NDistribUnit.Common.Server.ConnectionTracking.Discovery;
using NDistribUnit.Integration.Tests.General;
using NUnit.Framework;

namespace NDistribUnit.Integration.Tests.Communication
{
    [TestFixture]
    public class DiscoveryConnectionTrackersStateTests : ConnectionTrackersStateTestsBase
    {
        protected override IntegrationTestsFixture GetTestFixture()
        {
            var fixture = new IntegrationTestsFixture();
            fixture.SetConnectionsTracker(typeof(DiscoveryConnectionsTracker<>));
            return fixture;
        }
    }
}
