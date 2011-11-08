using NUnit.Framework;

namespace NDistribUnit.SampleTestAssembly.CategorizedTests
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture, Category("Group1")]
    public class TestFixtureWithGroup1CategoryAndCategoriesOnTests
    {
        [Test]
        public void TestWithNoCategories()
        {
        }

        [Test, Category("Group1")]
        public void TestWithGroup1Category()
        {}

        [Test, Category("Group2")]
        public void TestWithGroup2Category()
        {}

        [Test, Category("Group1"), Category("Group2")]
        public void TestWithGroup1Group2Category()
        {}
    }
}