using System;
using System.Collections;
using System.ServiceModel;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.TestExecution;
using NUnit.Core;

namespace NDistribUnit.Common.Client
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceContract]
    [ServiceKnownType(typeof(ArrayList))]
    public interface IClient
    {
        //void TestProgress(TestResult result);

        /// <summary>
        /// Notifies that the test has completed.
        /// </summary>
        /// <param name="receivedResult">The result.</param>
        /// <param name="isCompleted"></param>
        [OperationContract]
        void NotifyTestProgressChanged(TestResult receivedResult, bool isCompleted);

        /// <summary>
        /// Gets the project.
        /// </summary>
        /// <param name="testRunId">The test run id.</param>
        /// <returns></returns>
        [OperationContract]
        PackedProject GetPackedProject(Guid testRunId);

        /// <summary>
        /// Pings the tracking side
        /// </summary>
        /// <param name="pingInterval"></param>
        /// <returns>Anything (including null) if everything is ok, throws exception otherwise</returns>
        [OperationContract]
        PingResult Ping(TimeSpan pingInterval);
    }
}