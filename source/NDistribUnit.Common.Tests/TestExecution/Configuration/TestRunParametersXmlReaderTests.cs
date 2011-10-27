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
    </specialFailureHandling>
    <maximumAgentsCount>90</maximumAgentsCount>
</ndistribunit-project>";

            var result = new TestRunParametersXmlReader().Read(xml);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.MaximumAgentsCount, Is.EqualTo(90));
            Assert.That(result.SpecialHandlings.Count, Is.EqualTo(2));

            AssertFailureHandling(result.SpecialHandlings[0], "This is the message", FailureMessageType.Regex, 99);
            AssertFailureHandling(result.SpecialHandlings[1], "This is message 2", FailureMessageType.ContainsText, 98);
        }

        private void AssertFailureHandling(TestRunFailureSpecialHandling specialHandling, 
            string expectedMessage, 
            FailureMessageType expectedMessageType, 
            int expectedretryCount)
        {
            Assert.That(specialHandling.FailureMessage, Is.EqualTo(expectedMessage));
            Assert.That(specialHandling.FailureMessageType, Is.EqualTo(expectedMessageType));
            Assert.That(specialHandling.RetryCount, Is.EqualTo(expectedretryCount));
            
        }
    }
}