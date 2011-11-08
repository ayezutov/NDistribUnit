using System;
using System.Collections.Concurrent;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Server.ConnectionTracking;
using NDistribUnit.Common.Extensions;
using NDistribUnit.Common.TestExecution.Data;

namespace NDistribUnit.Common.TestExecution.Storage
{
    /// <summary>
    /// A queue for holding test runs
    /// </summary>
    public class RequestsStorage : IRequestsStorage
    {
        private readonly ConcurrentDictionary<Guid, TestRunRequest> requests = new ConcurrentDictionary<Guid, TestRunRequest>();
        private readonly PingableCollection<TestRunRequest> pingable;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestsStorage"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="log">The log.</param>
        public RequestsStorage(IConnectionsHostOptions options, ILog log)
        {
            pingable = new PingableCollection<TestRunRequest>(options, log);
            pingable.Removed += (sender, args) => args.Data.RemoveClient();
        }

        /// <summary>
        /// Occurs when a request is added to the queue.
        /// </summary>
        public event EventHandler<EventArgs<TestRunRequest>> Added;
        /// <summary>
        /// Adds the request.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public TestRunRequest AddOrUpdate(TestRun testRun, ITestRunnerClient client)
        {
            var exists = true;
            var addedRequest = requests.GetOrAdd(testRun.Id, guid =>
                                                      {
                                                          exists = false;
                                                          return new TestRunRequest(testRun, client);
                                                      });

            if (exists)
                addedRequest.SetClient(client);
            else
            {
                addedRequest.Status = TestRunRequestStatus.Received;
                Added.SafeInvokeAsync(this, addedRequest);
            }

            pingable.Add(addedRequest);

            return addedRequest;
        }

        /// <summary>
        /// Removes the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void Remove(TestRunRequest request)
        {
            TestRunRequest removed;
            requests.TryRemove(request.TestRun.Id, out removed);
        }
    }
}