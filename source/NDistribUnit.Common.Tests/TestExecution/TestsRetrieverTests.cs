using System.IO;
using System.Linq;
using System.Reflection;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.TestExecution.Data;
using NDistribUnit.Common.TestExecution.Storage;
using NDistribUnit.SampleTestAssembly.CategorizedTests;
using NUnit.Framework;

namespace NDistribUnit.Common.Tests.TestExecution
{
    [TestFixture]
    public class TestsRetrieverTests
    {
        private TestsRetriever retriever;
        private TestProject project;
        private TestRunRequest request;

        [SetUp]
        public void Init()
        {
            retriever = new TestsRetriever(new NUnitInitializer());
            string targetAssembly = typeof(TestFixtureWithCategoriesOnTests).Assembly.Location;
            project = new TestProject(Path.GetDirectoryName(targetAssembly));
            request = new TestRunRequest(new TestRun
                                                 {
                                                     TestOptions = new ClientParameters
                                                     {
                                                     }
                                                 }, null);
            request.TestRun.TestOptions.AssembliesToTest.Add(targetAssembly);
        }

        [TearDown]
        public void Dispose()
        {
        }

        [TestFixtureSetUp]
        public void InitOnce()
        {
        }

        [TestFixtureTearDown]
        public void DisposeOnce()
        {
        }

        [Test]
        public void EnsureAllSuitesAreLoadedWhenNoCategoriesAreSelected()
        {
            var testUnits = retriever.Get(project, request);
            Assert.That(testUnits, Is.Not.Null);
            Assert.That(testUnits.Any(u => u.UniqueTestId.Equals(typeof(TestFixtureWithCategoriesOnTests).FullName)), 
                Is.True);
            Assert.That(testUnits.Any(u => u.UniqueTestId.Equals(typeof(TestFixtureWithGroup1CategoryAndCategoriesOnTests).FullName)), 
                Is.True);
            Assert.That(testUnits.Any(u => u.UniqueTestId.Equals(typeof(TestFixtureWithNoGroup1CategoryOnTestsOrSelf).FullName)), 
                Is.True);
        }

        [Test]
        public void EnsureOnlyParticularTestFixturesAreLoadedIfFilteredByGroup1()
        {
            request.TestRun.TestOptions.IncludeCategories = "Group1";

            var testUnits = retriever.Get(project, request);

            Assert.That(testUnits, Is.Not.Null);
            Assert.That(testUnits.Any(u => u.UniqueTestId.Equals(typeof(TestFixtureWithCategoriesOnTests).FullName)), 
                Is.True);
            Assert.That(testUnits.Any(u => u.UniqueTestId.Equals(typeof(TestFixtureWithGroup1CategoryAndCategoriesOnTests).FullName)), 
                Is.True);
            Assert.That(testUnits.Any(u => u.UniqueTestId.Equals(typeof(TestFixtureWithNoGroup1CategoryOnTestsOrSelf).FullName)), 
                Is.False);

        }

        [Test]
        public void EnsureOnlyParticularTestFixturesAreLoadedIfFilteredByGroup3()
        {
            request.TestRun.TestOptions.IncludeCategories = "Group3";

            var testUnits = retriever.Get(project, request);

            Assert.That(testUnits, Is.Not.Null);
            Assert.That(testUnits.Any(u => u.UniqueTestId.Equals(typeof(TestFixtureWithCategoriesOnTests).FullName)), 
                Is.False);
            Assert.That(testUnits.Any(u => u.UniqueTestId.Equals(typeof(TestFixtureWithGroup1CategoryAndCategoriesOnTests).FullName)), 
                Is.False);
            Assert.That(testUnits.Any(u => u.UniqueTestId.Equals(typeof(TestFixtureWithNoGroup1CategoryOnTestsOrSelf).FullName)), 
                Is.True);

        }

        [Test]
        public void EnsureOnlyParticularTestFixturesAreLoadedIfFilteredByExcludeGroup1()
        {
            request.TestRun.TestOptions.ExcludeCategories = "Group1";

            var testUnits = retriever.Get(project, request);

            Assert.That(testUnits, Is.Not.Null);
            Assert.That(testUnits.Any(u => u.UniqueTestId.Equals(typeof(TestFixtureWithCategoriesOnTests).FullName)), 
                Is.True); // there are some tests without Group1
            Assert.That(testUnits.Any(u => u.UniqueTestId.Equals(typeof(TestFixtureWithGroup1CategoryAndCategoriesOnTests).FullName)), 
                Is.False);
            Assert.That(testUnits.Any(u => u.UniqueTestId.Equals(typeof(TestFixtureWithNoGroup1CategoryOnTestsOrSelf).FullName)), 
                Is.True);

        }
    }
}