using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.Server.AgentsTracking.AgentsProviders.Announcement;
using NDistribUnit.Integration.Tests.Infrastructure;
using NUnit.Framework;

namespace NDistribUnit.Integration.Tests.Tests.Communication
{
    [TestFixture, Explicit]
    public class AnnouncementAgentsTrackersStateTests : AgentsTrackersStateTestsBase
    {
        protected override void ConfigureSystem(NDistribUnitTestSystemFluent system)
        {
            system
                .ActAsRealSystemWithOpeningPorts()
                .SetConnectionsTracker<AnnouncementAgentsProvider>();
        }
    }
}