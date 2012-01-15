using System;
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
	}
}