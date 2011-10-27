using System;
using System.Collections.Concurrent;
using NDistribUnit.Common.Server.Services;
using NDistribUnit.Common.Extensions;

namespace NDistribUnit.Common.TestExecution.Preparation
{
    /// <summary>
    /// A queue for holding test runs
    /// </summary>
    public class TestRunRequestsStorage
    {
        private ConcurrentDictionary<Guid, TestRunRequest> requests = new ConcurrentDictionary<Guid, TestRunRequest>();

        /// <summary>
        /// Occurs when [added].
        /// </summary>
        public event EventHandler<EventArgs<TestRunRequest>> Added;
        /// <summary>
        /// Adds the request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void Add(TestRunRequest request)
        {
            var exists = true;
            var addedRequest = requests.GetOrAdd(request.TestRun.Id, guid =>
                                                      {
                                                          exists = false;
                                                          return request;
                                                      });

            if (exists)
                addedRequest.SetClient(request.Client);
            else
            {
                addedRequest.Status = TestRunRequestStatus.Received;
                Added.SafeInvokeAsync(this, request);
            }
        }
    }
}