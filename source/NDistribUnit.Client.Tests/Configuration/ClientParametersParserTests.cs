using NDistribUnit.Client.Configuration;
using NUnit.Framework;

namespace NDistribUnit.Client.Tests.Configuration
{
    [TestFixture]
    public class ClientParametersParserTests
    {
        private ClientParametersParser parser;

        [SetUp]
        public void Init()
        {
            parser = new ClientParametersParser();
        }

        [Test]
        public void ParsingSingleAssemblyNameResultsInAssemblyNamePopulated()
        {
            ClientParameters parameters = parser.Parse(new[] {"some.tests.assembly.dll"});

            Assert.That(parameters.AssembliesToTest, Is.EquivalentTo(new[] { "some.tests.assembly.dll" }));
        }

        [Test]
        public void ParsingMultipleAssemblyNamesResultsInAssemblyNamesPopulated()
        {
            ClientParameters parameters = parser.Parse(new[] { "some.tests.assembly23.dll", "some.tests.assembly.dll" });

            Assert.That(parameters.AssembliesToTest, Is.EquivalentTo(new[] { "some.tests.assembly.dll", "some.tests.assembly23.dll" }));
        }

        [Test]
        public void UnknownParametersAreIgnored()
        {
            ClientParameters parameters = parser.Parse(new[] { "some.tests.assembly23.dll", "some.tests.assembly.dll", "/fake:someValue", "/fake2", "someOtherValue" });

            Assert.That(parameters.AssembliesToTest, Is.EquivalentTo(new[] { "some.tests.assembly.dll", "some.tests.assembly23.dll" }));
        }

        [Test]
        public void XmlParameterIsCatched()
        {
            ClientParameters parameters = parser.Parse(new[] { "some.tests.assembly23.dll", "some.tests.assembly.dll", "/xml:xml.file" });

            Assert.That(parameters.AssembliesToTest, Is.EquivalentTo(new[] { "some.tests.assembly.dll", "some.tests.assembly23.dll" }));
            Assert.That(parameters.XmlFileName, Is.EqualTo("xml.file"));
            Assert.That(parameters.NoShadow, Is.False);
        }

        [Test]
        public void NoShadowParameterIsCatched()
        {
            ClientParameters parameters = parser.Parse(new[] { "some.tests.assembly23.dll", "/noshadow", "some.tests.assembly.dll", "/xml:xml.file" });

            Assert.That(parameters.AssembliesToTest, Is.EquivalentTo(new[] { "some.tests.assembly.dll", "some.tests.assembly23.dll" }));
            Assert.That(parameters.XmlFileName, Is.EqualTo("xml.file"));
            Assert.That(parameters.NoShadow, Is.True);
        }

        [Test]
        public void ParametersOrderDoesNotMakeDifference()
        {
            ClientParameters parameters = parser.Parse(new[] { "/noshadow", "/xml:xml.file", "some.tests.assembly23.dll", "some.tests.assembly.dll" });

            Assert.That(parameters.AssembliesToTest, Is.EquivalentTo(new[] { "some.tests.assembly.dll", "some.tests.assembly23.dll" }));
            Assert.That(parameters.XmlFileName, Is.EqualTo("xml.file"));
            Assert.That(parameters.NoShadow, Is.True);
        }
    }
}