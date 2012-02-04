using System;
using System.Threading;
using NDistribUnit.Common.Common;
using NDistribUnit.Common.Contracts.DataContracts;
using NUnit.Core;

namespace NDistribUnit.Common.TestExecution.Data
{
	/// <summary>
	/// 
	/// </summary>
	public class TestRunRequest
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestRunRequest"/> class.
		/// </summary>
		/// <param name="testRun">The test run.</param>
		public TestRunRequest(TestRun testRun)
		{
			TestRun = testRun;
		    RequestTime = DateTime.UtcNow;
            PipeToClient = new CrossThreadPipe<TestResult>(TimeSpan.FromSeconds(5), list =>
                                                                                        {
                                                                                            for (int i = list.Count - 1; i > 0; i--)
                                                                                            {
                                                                                                new TestResultsProcessor().Merge(list[i], list[0]);
                                                                                            }
                                                                                        });
            Statistics = new RequestStatistics();
		}

		/// <summary>
		/// Gets or sets the test run.
		/// </summary>
		/// <value>
		/// The test run.
		/// </value>
		public TestRun TestRun { get; private set; }
        
	    /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
	    public TestRunRequestStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the request time.
        /// </summary>
        /// <value>
        /// The request time.
        /// </value>
	    public DateTime RequestTime { get; private set; }

        /// <summary>
        /// Gets or sets the configuration setup.
        /// </summary>
        /// <value>
        /// The configuration setup.
        /// </value>
	    public DistributedConfigurationSetup ConfigurationSetup { get; set; }

        /// <summary>
        /// Gets the results pipe to client.
        /// </summary>
	    public CrossThreadPipe<TestResult> PipeToClient { get; private set; }

	    /// <summary>
	    /// Gets the statistics.
	    /// </summary>
        public RequestStatistics Statistics { get; private set; }
	}

    /// <summary>
    /// 
    /// </summary>
    public class RequestStatistics
    {
        private int totalCount;
        private int reprocessedCount;

        /// <summary>
        /// Gets the reprocessed count.
        /// </summary>
        public int ReprocessedCount
        {
            get { return reprocessedCount; }
        }

        /// <summary>
        /// Gets the total.
        /// </summary>
        public int Total
        {
            get { return totalCount; }
        }

        /// <summary>
        /// Initializes the specified test units count.
        /// </summary>
        /// <param name="count">The count.</param>
        public void Initialize(int count)
        {
            totalCount = count;
        }

        /// <summary>
        /// Gets the count and increase.
        /// </summary>
        /// <returns></returns>
        public int GetCountAndIncrement()
        {
            return Interlocked.Add(ref totalCount, 1);
        }

        /// <summary>
        /// Registers the reprocessing.
        /// </summary>
        public void RegisterReprocessing()
        {
            Interlocked.Increment(ref reprocessedCount);
        }
    }
}