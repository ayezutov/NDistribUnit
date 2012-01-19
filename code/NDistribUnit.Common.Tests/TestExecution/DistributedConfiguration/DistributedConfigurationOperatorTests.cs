using System.IO;
using System.Linq;
using System.Xml.Linq;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.TestExecution.DistributedConfiguration;
using NUnit.Framework;

namespace NDistribUnit.Common.Tests.TestExecution.DistributedConfiguration
{
    [TestFixture]
    public class DistributedConfigurationOperatorTests
    {
        private DistributedConfigurationOperator @operator;

        [SetUp]
        public void Init()
        {
            @operator = new DistributedConfigurationOperator();
        }

        [Test]
        public void ReadingSimilarCommentsDoesNotCauseAFailure()
        {
            var doc = new XDocument(new XElement("configuration"),
                                    new XComment(
                                        "NDistribUnit. This is a test. {Name: 'Sequence', Parameters:{Start:3, Step: 2}}"));

            var result = new DistributedConfigurationSetup();

            @operator.FillSetupInstance(doc, result);

            Assert.That(result.Variables.Count, Is.EqualTo(0));
        }

        [Test]
        public void ReadSequenceVariable()
        {
            var doc = new XDocument(new XElement("configuration"),
                                    new XComment(
                                        "NDistribUnit.Variable { Name:'Id', Type:\"Sequence\", TypeArguments:{Start:3,Step:2,NotExistentProperty:'Xaxa',Maximum:100}}"));

            var result = new DistributedConfigurationSetup();

            @operator.FillSetupInstance(doc, result);

            Assert.That(result.Variables.Count, Is.EqualTo(1));
        }

        [Test]
        public void ReplaceInstructionsWorksFine()
        {
            var doc =
                XDocument.Load(
                    new StringReader(
                        @"
<configuration>
    <appSettings>
        <add name=""first"" connectionString=""previous1""/>
        <add name=""second"" connectionString=""previous2""/>
        <!--NDistribUnit.Replace {XPath:""../add[@name='second']/@connectionString"", Value:'otherValue'}-->
        <add name=""third"" connectionString=""previous3""/>
    </appSettings>
</configuration>
"));
            @operator.SubstitutePlaceHolders(doc, new DistributedConfigurationSubstitutions());

            var addElements = doc.Descendants("add").ToList();

            Assert.That(addElements[0].Attributes("name").FirstOrDefault().Value, Is.EqualTo("first"));
            Assert.That(addElements[0].Attributes("connectionString").FirstOrDefault().Value, Is.EqualTo("previous1"));

            Assert.That(addElements[1].Attributes("name").FirstOrDefault().Value, Is.EqualTo("second"));
            Assert.That(addElements[1].Attributes("connectionString").FirstOrDefault().Value, Is.EqualTo("otherValue"));

            Assert.That(addElements[2].Attributes("name").FirstOrDefault().Value, Is.EqualTo("third"));
            Assert.That(addElements[2].Attributes("connectionString").FirstOrDefault().Value, Is.EqualTo("previous3"));
        }

        [Test]
        public void ReplaceInstructionsWithFilePathsWorkFine()
        {
            var doc =
                XDocument.Load(
                    new StringReader(
                        @"
<configuration>
    <appSettings>
        <add name=""second"" connectionString=""previous2""/>
        <!--NDistribUnit.Replace {XPath:""../add[@name='second']/@connectionString"", Value:'C:\\Temp\\Somefolder\\'}-->
    </appSettings>
</configuration>
"));
            @operator.SubstitutePlaceHolders(doc, new DistributedConfigurationSubstitutions());

            var addElements = doc.Descendants("add").ToList();

            Assert.That(addElements[0].Attributes("name").FirstOrDefault().Value, Is.EqualTo("second"));
            Assert.That(addElements[0].Attributes("connectionString").FirstOrDefault().Value, Is.EqualTo(@"C:\Temp\Somefolder\"));
        }

        [Test]
        public void SubstituteVariableWorksFine()
        {
            var doc =
                XDocument.Load(
                    new StringReader(
                        @"
<configuration>
    <appSettings>
        <add name=""first"" connectionString=""${Id}previous${Id}""/>
        <add name=""second"" connectionString=""previous${Id}${Configuration}""/>
        <add name=""third"" connectionString=""pre$$vious$${Id}""/>
    </appSettings>
    <element><subElement>${Configuration}${Id}${id}</subElement></element>
</configuration>
"));
            @operator.SubstitutePlaceHolders(doc, new DistributedConfigurationSubstitutions()
                                                      {
                                                          Variables =
                                                              {
                                                                  new DistributedConfigurationVariablesValue("Id", "25"),
                                                                  new DistributedConfigurationVariablesValue(
                                                                      "Configuration", "STG")
                                                              }
                                                      });

            var addElements = doc.Descendants("add").ToList();

            Assert.That(addElements[0].Attributes("name").FirstOrDefault().Value, Is.EqualTo("first"));
            Assert.That(addElements[0].Attributes("connectionString").FirstOrDefault().Value, Is.EqualTo("25previous25"));

            Assert.That(addElements[1].Attributes("name").FirstOrDefault().Value, Is.EqualTo("second"));
            Assert.That(addElements[1].Attributes("connectionString").FirstOrDefault().Value,
                        Is.EqualTo("previous25STG"));

            Assert.That(addElements[2].Attributes("name").FirstOrDefault().Value, Is.EqualTo("third"));
            Assert.That(addElements[2].Attributes("connectionString").FirstOrDefault().Value,
                        Is.EqualTo("pre$$vious${Id}"));

            Assert.That(doc.Descendants("subElement").FirstOrDefault().Value, Is.EqualTo("STG25${id}"));
        }


        [Test]
        public void VariableInReplaceInstructionWorksFine()
        {
            var doc =
                XDocument.Load(
                    new StringReader(
                        @"
<configuration>
    <appSettings>
        <add name=""first"" connectionString=""previous1""/>
        <add name=""second"" connectionString=""previous2""/>
        <!--NDistribUnit.Replace {XPath:""../add[@name='second']/@connectionString"", Value:'otherValue${Id}${Configuration}'}-->
        <add name=""third"" connectionString=""previous3""/>
    </appSettings>
</configuration>
"));
            @operator.SubstitutePlaceHolders(doc, new DistributedConfigurationSubstitutions()
                                                      {
                                                          Variables =
                                                              {
                                                                  new DistributedConfigurationVariablesValue("Id", "26"),
                                                                  new DistributedConfigurationVariablesValue("Configuration", "STG")
                                                              }
                                                      });

            var addElements = doc.Descendants("add").ToList();

            Assert.That(addElements[1].Attributes("name").FirstOrDefault().Value, Is.EqualTo("second"));
            Assert.That(addElements[1].Attributes("connectionString").FirstOrDefault().Value, Is.EqualTo("otherValue26STG"));
        }
    }
}