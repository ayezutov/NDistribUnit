using NUnit.Framework;

namespace NDistribUnit.SampleTestAssembly.CategorizedTests
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class TestFixtureWithNoGroup1CategoryOnTestsOrSelf
    {
        [Test]
        public void TestWithNoCategories()
        {
        }

        [Test, Category("Group3")]
        public void TestWithGroup1Category()
        {}

        [Test, Category("Group2")]
        public void TestWithGroup2Category()
        {}

        [Test, Category("Group3"), Category("Group2")]
        public void TestWithGroup1Group2Category()
        {}
    }
}