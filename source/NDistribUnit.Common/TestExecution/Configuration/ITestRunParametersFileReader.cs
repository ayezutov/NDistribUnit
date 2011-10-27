namespace NDistribUnit.Common.TestExecution.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITestRunParametersFileReader
    {
        /// <summary>
        /// Reads the NDistribUnit test parameters from the specified file .
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        TestRunParameters Read(string fileName);
    }
}