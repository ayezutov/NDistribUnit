using NDistribUnit.Common.Client;
using NUnit.Framework;

namespace NDistribUnit.Client.Tests.Configuration
{
    [TestFixture]
    public class ClientParametersParserTests
    {
        [SetUp]
        public void Init()
        {
        }

        [Test]
        public void ParsingSingleAssemblyNameResultsInAssemblyNamePopulated()
        {
            ClientParameters parameters = ClientParameters.Parse(new[] { "some.tests.assembly.dll" });

            Assert.That(parameters.AssembliesToTest, Is.EquivalentTo(new[] { "some.tests.assembly.dll" }));
        }

        [Test]
        public void ParsingMultipleAssemblyNamesResultsInAssemblyNamesPopulated()
        {
            ClientParameters parameters = ClientParameters.Parse(new[] { "some.tests.assembly23.dll", "some.tests.assembly.dll" });

            Assert.That(parameters.AssembliesToTest, Is.EquivalentTo(new[] { "some.tests.assembly.dll", "some.tests.assembly23.dll" }));
        }

        [Test]
        public void UnknownParametersAreIgnored()
        {
            ClientParameters parameters = ClientParameters.Parse(new[] { "some.tests.assembly23.dll", "some.tests.assembly.dll", "/fake:someValue", "/fake2", "someOtherValue" });

            Assert.That(parameters.AssembliesToTest, Is.EquivalentTo(new[] { "some.tests.assembly.dll", "some.tests.assembly23.dll" }));
        }

        [Test]
        public void XmlParameterIsCatched()
        {
            ClientParameters parameters = ClientParameters.Parse(new[] { "some.tests.assembly23.dll", "some.tests.assembly.dll", "/xml:xml.file" });

            Assert.That(parameters.AssembliesToTest, Is.EquivalentTo(new[] { "some.tests.assembly.dll", "some.tests.assembly23.dll" }));
            Assert.That(parameters.XmlFileName, Is.EqualTo("xml.file"));
            Assert.That(parameters.NoShadow, Is.False);
        }

        [Test]
        public void NoShadowParameterIsCatched()
        {
            ClientParameters parameters = ClientParameters.Parse(new[] { "some.tests.assembly23.dll", "/noshadow", "some.tests.assembly.dll", "/xml:xml.file" });

            Assert.That(parameters.AssembliesToTest, Is.EquivalentTo(new[] { "some.tests.assembly.dll", "some.tests.assembly23.dll" }));
            Assert.That(parameters.XmlFileName, Is.EqualTo("xml.file"));
            Assert.That(parameters.NoShadow, Is.True);
        }

        [Test]
        public void ParametersOrderDoesNotMakeDifference()
        {
            ClientParameters parameters = ClientParameters.Parse(new[] { "/noshadow", "/xml:xml.file", "some.tests.assembly23.dll", "some.tests.assembly.dll" });

            Assert.That(parameters.AssembliesToTest, Is.EquivalentTo(new[] { "some.tests.assembly.dll", "some.tests.assembly23.dll" }));
            Assert.That(parameters.XmlFileName, Is.EqualTo("xml.file"));
            Assert.That(parameters.NoShadow, Is.True);
        }

        [Test]
        public void IncludeCategoriesAreParsedCorrectly()
        {
            ClientParameters parameters = ClientParameters.Parse(new[] { "/noshadow", "/xml:xml.file", "some.tests.assembly23.dll", "some.tests.assembly.dll", "/include:Cat1, Another Category,SomeCategory," });

            Assert.That(parameters.IncludeCategories, Is.EquivalentTo(new[] { "Cat1","Another Category","SomeCategory" }));
        }

        [Test]
        public void ExcludeCategoriesAreParsedCorrectly()
        {
            ClientParameters parameters = ClientParameters.Parse(new[] { "/noshadow", "/xml:xml.file", "some.tests.assembly23.dll", "some.tests.assembly.dll", "/exclude:Cat1, Another Category,SomeCategory," });

            Assert.That(parameters.ExcludeCategories, Is.EquivalentTo(new[] { "Cat1","Another Category","SomeCategory" }));
        }

        [Test]
        public void IncludeCategoriesAreEmptyIfNotProvided()
        {
            ClientParameters parameters = ClientParameters.Parse(new[] { "/noshadow", "/xml:xml.file", "some.tests.assembly23.dll", "some.tests.assembly.dll"});

            Assert.That(parameters.IncludeCategories, Is.EquivalentTo(new string[0]));
        }

        [Test]
        public void ExcludeCategorieAreEmptyIfNotProvided()
        {
            ClientParameters parameters = ClientParameters.Parse(new[] { "/noshadow", "/xml:xml.file", "some.tests.assembly23.dll", "some.tests.assembly.dll"});

            Assert.That(parameters.ExcludeCategories, Is.EquivalentTo(new string[0]));
        }

        [Test]
        public void AliasIsParsed()
        {
            ClientParameters parameters = ClientParameters.Parse(new[] { "/noshadow", "/alias:O2I-UI-Tests-v.12-b.64.00"});

            Assert.That(parameters.Alias, Is.EqualTo("O2I-UI-Tests-v.12-b.64.00"));
        }

        [Test]
        public void AliasIsNullByDefault()
        {
            ClientParameters parameters = ClientParameters.Parse(new[] { "/noshadow"});

            Assert.That(parameters.Alias, Is.Null);
        }
    }
}