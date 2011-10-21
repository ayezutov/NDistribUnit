using NDistribUnit.Integration.Tests.Infrastructure;
using NUnit.Framework;

namespace NDistribUnit.Integration.Tests.Tests.Communication
{
    [TestFixture, Explicit]
    public class TestingConnectionTrackerTests : ConnectionTrackersStateTestsBase
    {
        protected override void ConfigureSystem(NDistribUnitTestSystem system)
        {}
    }
}