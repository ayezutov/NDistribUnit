using NDistribUnit.Common.Communication.ConnectionTracking.Announcement;
using NDistribUnit.Integration.Tests.General;
using NUnit.Framework;

namespace NDistribUnit.Integration.Tests.Communication
{
    [TestFixture]
    public class AnnouncementConnectionTrackersStateTests : ConnectionTrackersStateTestsBase
    {
        protected override IntegrationTestsFixture GetTestFixture()
        {
            var fixture = new IntegrationTestsFixture();
            fixture.SetConnectionsTracker(typeof(AnnouncementConnectionsTracker<>));
            return fixture;
        }
    }
}