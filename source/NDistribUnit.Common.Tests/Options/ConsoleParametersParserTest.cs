using NDistribUnit.Common.Options;
using NUnit.Framework;
using System.Linq;

namespace NDistribUnit.Common.Tests.Options
{
    [TestFixture]
    public class ConsoleParametersParserTest
    {
        [Test]
        public void NamedParametersAreParsedCorrectly()
        {
            string tagValue = null;
            int intValue = 0;
            var consoleParametersParser = new ConsoleParametersParser()
                                              {
                                                  {"tag", (string tag) => tagValue = tag},
                                                  {"int", (int value) => intValue = value}
                                              };

            var result = consoleParametersParser.Parse(new[]{"/int:40", "/tag:asdgf", "/tag2:bsdfg"});

            Assert.That(tagValue, Is.EqualTo("asdgf"));
            Assert.That(intValue, Is.EqualTo(40));
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("tag2"));
            Assert.That(result[0].Value, Is.EqualTo("bsdfg"));
        }
        
        [Test]
        public void UnnamedParametersAreParsedCorrectly()
        {
            string unnamed = null;
            var consoleParametersParser = new ConsoleParametersParser()
                                              {
                                                  {ConsoleOption.UnnamedOptionName, (string value) => unnamed = value},
                                              };

            var result = consoleParametersParser.Parse(new[]{"/int:40", "/tag:asdgf", "not named", "/tag2:bsdfg"});

            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(unnamed, Is.EqualTo("not named"));
        }

        [Test]
        public void UnregisterdFlagBeforeNameValueIsParsedCorrectly()
        {
            var consoleParametersParser = new ConsoleParametersParser();

            var result = consoleParametersParser.Parse(new[]{"/int:40", "/tag:asdgf", "/flag", "/tag2:bsdfg"});

            Assert.That(result.Select(r => r.Name).ToArray(), Is.EquivalentTo(new[]{"int", "tag", "flag", "tag2"}));
        }
    }
}