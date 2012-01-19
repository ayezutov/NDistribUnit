using NUnit.Framework;

namespace NDistribUnit.SampleTestAssembly.CategorizedTests
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class TestFixtureWithCategoriesOnTests
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