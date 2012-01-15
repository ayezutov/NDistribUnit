using NDistribUnit.Common.TestExecution.Configuration;
using NUnit.Framework;

namespace NDistribUnit.Common.Tests.TestExecution.Configuration
{
    [TestFixture]
    public class TestRunParametersXmlReaderTests
    {
        [Test]
        public void CanReadValidConfiguration()
        {
            var xml = @"
<ndistribunit-project>
    <specialFailureHandling>
        <failure message=""This is the message"" messageType=""Regex"" retryCount=""99""/>
        <failure message=""This is message 2"" messageType=""ContainsText"" retryCount=""98""/>
        <failure stackTrace=""This is stack trace"" stackTraceType=""ContainsText"" retryCount=""97""/>
        <failure stackTrace=""This is stack trace 2"" stackTraceType=""Regex"" message=""This is message 3"" messageType=""ContainsText"" retryCount=""96""/>
    </specialFailureHandling>
    <maximumAgentsCount>90</maximumAgentsCount>
</ndistribunit-project>";

            var result = new TestRunParametersXmlReader().Read(xml);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.MaximumAgentsCount, Is.EqualTo(90));
            Assert.That(result.SpecialHandlings.Count, Is.EqualTo(4));

            AssertFailureHandling(result.SpecialHandlings[0], "This is the message", MatchType.Regex, null, MatchType.None, 99);
            AssertFailureHandling(result.SpecialHandlings[1], "This is message 2", MatchType.ContainsText, null, MatchType.None, 98);
            AssertFailureHandling(result.SpecialHandlings[2], null, MatchType.None, "This is stack trace", MatchType.ContainsText, 97);
            AssertFailureHandling(result.SpecialHandlings[3], "This is message 3", MatchType.ContainsText, "This is stack trace 2", MatchType.Regex, 96);
        }


        private void AssertFailureHandling(TestRunFailureSpecialHandling specialHandling, 
            string expectedMessage, 
            MatchType expectedMessageType, 
            string expectedStackTrace,
            MatchType expectedStackTraceType,
            int expectedretryCount)
        {
            Assert.That(specialHandling.FailureMessage, Is.EqualTo(expectedMessage));
            Assert.That(specialHandling.FailureStackTrace, Is.EqualTo(expectedStackTrace));
            Assert.That(specialHandling.FailureMessageType, Is.EqualTo(expectedMessageType));
            Assert.That(specialHandling.FailureStackTraceType, Is.EqualTo(expectedStackTraceType));
            Assert.That(specialHandling.RetryCount, Is.EqualTo(expectedretryCount));
            
        }
    }
}