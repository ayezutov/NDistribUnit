using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using NUnit.Core;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public class TestResultsSerializer : ITestResultsSerializer
    {
        /// <summary>
        /// Gets the XML.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public string GetXml(TestResult result)
        {
            var stringBuilder = new StringBuilder();
            var writer = new StringWriter(stringBuilder);
            var resultsWriter = new NUnit.Util.XmlResultWriter(writer);
            resultsWriter.SaveTestResult(result);
            writer.Close();
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Parses the specified XML into test result.
        /// </summary>
        /// <param name="data"></param>
        public TestResult Deserialize(Stream data)
        {
            return (TestResult)new BinaryFormatter().Deserialize(data);
        }

        /// <summary>
        /// Parses the specified XML into test result.
        /// </summary>
        /// <param name="serialize">The serialize.</param>
        /// <param name="serializeTo">The serialize to.</param>
        /// <returns></returns>
        public void Serialize(TestResult serialize, Stream serializeTo)
        {
            new BinaryFormatter().Serialize(serializeTo, serialize);
        }
    }
}