using System;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.TestExecution.Data;

namespace NDistribUnit.Common.TestExecution.Storage
{
    /// <summary>
    /// Stores the list of requests, which were sent to server
    /// </summary>
    public interface IRequestsStorage
    {
        /// <summary>
        /// Occurs when a request is added to the queue.
        /// </summary>
        event EventHandler<EventArgs<TestRunRequest>> Added;

        /// <summary>
        /// Adds the request.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        TestRunRequest AddOrUpdate(TestRun testRun, ITestRunnerClient client);

        /// <summary>
        /// Removes the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        void Remove(TestRunRequest request);
    }
}