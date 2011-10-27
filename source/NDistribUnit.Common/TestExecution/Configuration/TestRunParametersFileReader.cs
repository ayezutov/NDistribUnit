using System.IO;

namespace NDistribUnit.Common.TestExecution.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class TestRunParametersFileReader : ITestRunParametersFileReader
    {
        /// <summary>
        /// Reads the NDistribUnit test parameters from the specified file .
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
         public TestRunParameters Read(string fileName)
        {
            var configFile = new StreamReader(fileName);
            var xml = configFile.ReadToEnd();
            return new TestRunParametersXmlReader().Read(xml);
        }
    }
}