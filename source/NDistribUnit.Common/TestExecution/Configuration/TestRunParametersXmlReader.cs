using System.IO;
using System.Xml.Serialization;

namespace NDistribUnit.Common.TestExecution.Configuration
{
    /// <summary>
    /// Reads test run parameters from string
    /// </summary>
    public class TestRunParametersXmlReader
    {
        /// <summary>
        /// Reads the specified XML.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <returns></returns>
        public TestRunParameters Read(string xml)
        {
            var serializer = new XmlSerializer(typeof (TestRunParameters));
            return (TestRunParameters) serializer.Deserialize(new StringReader(xml));
        }
    }
}