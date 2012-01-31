using System.IO;
using System.Linq;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.TestExecution.Data;
using NDistribUnit.Common.TestExecution.Storage;
using NDistribUnit.Common.Updating;
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
            retriever = new TestsRetriever(new NUnitInitializer(), new BootstrapperParameters(), new ConsoleLog());
            string targetAssembly = typeof(TestFixtureWithCategoriesOnTests).Assembly.Location;
            project = new TestProject(Path.GetDirectoryName(targetAssembly));
            request = new TestRunRequest(new TestRun
                                                 {
                                                     NUnitParameters = new NUnitParameters()
                                                 });
            request.TestRun.NUnitParameters.AssembliesToTest.Add(targetAssembly);
        }

       
        [Test]
        public void EnsureAllSuitesAreLoadedWhenNoCategoriesAreSelected()
        {
            var testUnits = retriever.Get(project, request);
            Assert.That(testUnits, Is.Not.Null);
            Assert.That(testUnits.Any(u => u.Test.UniqueTestId.Equals(typeof(TestFixtureWithCategoriesOnTests).FullName)), 
                Is.True);
            Assert.That(testUnits.Any(u => u.Test.UniqueTestId.Equals(typeof(TestFixtureWithGroup1CategoryAndCategoriesOnTests).FullName)), 
                Is.True);
            Assert.That(testUnits.Any(u => u.Test.UniqueTestId.Equals(typeof(TestFixtureWithNoGroup1CategoryOnTestsOrSelf).FullName)), 
                Is.True);
        }

        [Test]
        public void EnsureOnlyParticularTestFixturesAreLoadedIfFilteredByGroup1()
        {
            request.TestRun.NUnitParameters.IncludeCategories = "Group1";

            var testUnits = retriever.Get(project, request);

            Assert.That(testUnits, Is.Not.Null);
            Assert.That(testUnits.Any(u => u.Test.UniqueTestId.Equals(typeof(TestFixtureWithCategoriesOnTests).FullName)), 
                Is.True);
            Assert.That(testUnits.Any(u => u.Test.UniqueTestId.Equals(typeof(TestFixtureWithGroup1CategoryAndCategoriesOnTests).FullName)), 
                Is.True);
            Assert.That(testUnits.Any(u => u.Test.UniqueTestId.Equals(typeof(TestFixtureWithNoGroup1CategoryOnTestsOrSelf).FullName)), 
                Is.False);

        }

        [Test]
        public void EnsureOnlyParticularTestFixturesAreLoadedIfFilteredByGroup3()
        {
            request.TestRun.NUnitParameters.IncludeCategories = "Group3";

            var testUnits = retriever.Get(project, request);

            Assert.That(testUnits, Is.Not.Null);
            Assert.That(testUnits.Any(u => u.Test.UniqueTestId.Equals(typeof(TestFixtureWithCategoriesOnTests).FullName)), 
                Is.False);
            Assert.That(testUnits.Any(u => u.Test.UniqueTestId.Equals(typeof(TestFixtureWithGroup1CategoryAndCategoriesOnTests).FullName)), 
                Is.False);
            Assert.That(testUnits.Any(u => u.Test.UniqueTestId.Equals(typeof(TestFixtureWithNoGroup1CategoryOnTestsOrSelf).FullName)), 
                Is.True);
        }

        [Test]
        public void EnsureOnlyParticularTestFixturesAreLoadedIfFilteredByExcludeGroup1()
        {
            request.TestRun.NUnitParameters.ExcludeCategories = "Group1";

            var testUnits = retriever.Get(project, request);

            Assert.That(testUnits, Is.Not.Null);
            Assert.That(testUnits.Any(u => u.Test.UniqueTestId.Equals(typeof(TestFixtureWithCategoriesOnTests).FullName)), 
                Is.True); // there are some tests without Group1
            Assert.That(testUnits.Any(u => u.Test.UniqueTestId.Equals(typeof(TestFixtureWithGroup1CategoryAndCategoriesOnTests).FullName)), 
                Is.False);
            Assert.That(testUnits.Any(u => u.Test.UniqueTestId.Equals(typeof(TestFixtureWithNoGroup1CategoryOnTestsOrSelf).FullName)), 
                Is.True);

        }

        [Test]
        public void EnsureTestByRunReturnsCorrectNumberOfTestsWhenSingleSuite()
        {
            request.TestRun.NUnitParameters.TestToRun = typeof(TestFixtureWithCategoriesOnTests).FullName;

            var testUnits = retriever.Get(project, request);

            Assert.That(testUnits, Is.Not.Null);
            Assert.That(testUnits.Count, Is.EqualTo(1));
        }

        [Test]
        public void EnsureTestByRunReturnsCorrectNumberOfTestsWhenMultipleSuites()
        {
            request.TestRun.NUnitParameters.TestToRun = typeof(TestFixtureWithCategoriesOnTests).Namespace;

            var testUnits = retriever.Get(project, request);

            Assert.That(testUnits, Is.Not.Null);
            Assert.That(testUnits.Count, Is.GreaterThan(1));
        }
    }
}