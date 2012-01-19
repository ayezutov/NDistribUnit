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
        /// <returns></returns>
        TestRunRequest AddOrUpdate(TestRun testRun);

        /// <summary>
        /// Removes the specified request.
        /// </summary>
        /// <param name="testRun"></param>
        TestRunRequest RemoveBy(TestRun testRun);

        /// <summary>
        /// Removes the specified request.
        /// </summary>
        /// <param name="testRun"></param>
        TestRunRequest GetBy(TestRun testRun);
    }
}