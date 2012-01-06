using System;
using NDistribUnit.Common.Contracts.DataContracts;

namespace NDistribUnit.Common.TestExecution.Data
{
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
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
    }
}