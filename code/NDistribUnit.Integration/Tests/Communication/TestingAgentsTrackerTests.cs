using NDistribUnit.Integration.Tests.Infrastructure;
using NUnit.Framework;

namespace NDistribUnit.Integration.Tests.Tests.Communication
{
    [TestFixture, Explicit]
    public class TestingAgentsTrackerTests : AgentsTrackersStateTestsBase
    {
        protected override void ConfigureSystem(NDistribUnitTestSystemFluent system)
        {}
    }
}