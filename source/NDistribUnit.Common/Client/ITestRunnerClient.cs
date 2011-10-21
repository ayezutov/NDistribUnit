using System.ServiceModel;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Common.Client
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceContract]
    public interface ITestRunnerClient: IPingable
    {
        //void TestProgress(TestResult result);

        /// <summary>
        /// Notifies that the test has completed.
        /// </summary>
        /// <param name="result">The result.</param>
        void NotifyTestCompleted(TestResult result);
    }
}