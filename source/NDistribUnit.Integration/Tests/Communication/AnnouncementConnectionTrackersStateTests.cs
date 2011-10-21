using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.Server.ConnectionTracking.Announcement;
using NDistribUnit.Integration.Tests.Infrastructure;
using NUnit.Framework;

namespace NDistribUnit.Integration.Tests.Tests.Communication
{
    [TestFixture, Explicit]
    public class AnnouncementConnectionTrackersStateTests : ConnectionTrackersStateTestsBase
    {
        protected override void ConfigureSystem(NDistribUnitTestSystem system)
        {
            system
                .ActAsRealSystemWithOpeningPorts()
                .SetConnectionsTracker<AnnouncementConnectionTracker<ITestRunnerAgent>>();
        }
    }
}