using System.IO;
using NUnit.Core;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITestResultsSerializer
    {
        /// <summary>
        /// Gets the XML.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        string GetXml(TestResult result);

        /// <summary>
        /// Parses the specified XML into test result.
        /// </summary>
        /// <param name="data"></param>
        TestResult Deserialize(Stream data);

        /// <summary>
        /// Parses the specified XML into test result.
        /// </summary>
        /// <param name="serialize">The serialize.</param>
        /// <param name="serializeTo">The serialize to.</param>
        void Serialize(TestResult serialize, Stream serializeTo);
    }
}