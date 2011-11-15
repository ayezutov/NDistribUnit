using System;
using System.Collections.Generic;
using NDistribUnit.Common.Contracts.DataContracts;

namespace NDistribUnit.Common.TestExecution
{
	/// <summary>
	/// Represents the smallest piece of work, which can be run by an agent
	/// </summary>
	[Serializable]
	public class TestUnit
	{
	    /// <summary>
        /// Initializes a new instance of the <see cref="TestUnit"/> class.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <param name="fullTestName">Full name of the test.</param>
	    public TestUnit(TestRun testRun, string fullTestName)
        {
            Run = testRun;
	        UniqueTestId = fullTestName;
        }

	    /// <summary>
		/// Gets or sets the test run.
		/// </summary>
		/// <value>
		/// The run.
		/// </value>
		public TestRun Run { get; private set; }

		/// <summary>
		/// Gets or sets the unique test id.
		/// </summary>
		/// <value>
		/// The unique test id.
		/// </value>
		public string UniqueTestId { get; private set; }
        
	    
	}

    /// <summary>
    /// 
    /// </summary>
    public class TestUnitWithMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestUnitWithMetadata"/> class.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <param name="fullTestName">Full name of the test.</param>
        public TestUnitWithMetadata(TestRun testRun, string fullTestName) 
        {
            Test = new TestUnit(testRun, fullTestName);
            Results = new List<TestResult>();
            SchedulerHints = new SchedulerHints();
        }

        /// <summary>
        /// Gets the test.
        /// </summary>
        public TestUnit Test { get; private set; }

        /// <summary>
        /// Gets or sets the results.
        /// </summary>
        /// <value>
        /// The results.
        /// </value>
        public List<TestResult> Results { get; private set; }

        /// <summary>
        /// Gets or sets the scheduler hints.
        /// </summary>
        /// <value>
        /// The scheduler hints.
        /// </value>
        public SchedulerHints SchedulerHints { get; private set; }
    }
}