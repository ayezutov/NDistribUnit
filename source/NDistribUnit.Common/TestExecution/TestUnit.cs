using System;
using System.Collections.Generic;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.TestExecution.Data;

namespace NDistribUnit.Common.TestExecution
{
	/// <summary>
	/// Represents the smallest piece of work, which can be run by an agent
	/// </summary>
	[Serializable]
	public class TestUnit
	{
        [NonSerialized]
        private List<TestResult> results;

	    /// <summary>
        /// Initializes a new instance of the <see cref="TestUnit"/> class.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <param name="fullTestName">Full name of the test.</param>
	    public TestUnit(TestRunRequest testRun, string fullTestName)
        {
            Request = testRun;
	        UniqueTestId = fullTestName;
            results = new List<TestResult>();
        }

	    /// <summary>
		/// Gets or sets the test run.
		/// </summary>
		/// <value>
		/// The run.
		/// </value>
		public TestRunRequest Request { get; set; }

		/// <summary>
		/// Gets or sets the unique test id.
		/// </summary>
		/// <value>
		/// The unique test id.
		/// </value>
		public string UniqueTestId { get; set; }
        
	    /// <summary>
        /// Gets or sets the results.
        /// </summary>
        /// <value>
        /// The results.
        /// </value>
        
	    public List<TestResult> Results
	    {
	        get { return results; }
	    }
	}
}