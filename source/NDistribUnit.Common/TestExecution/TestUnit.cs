using System;
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
        /// <param name="isSuite">if set to <c>true</c> [is suite].</param>
        /// <param name="testType">Type of the test.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        public TestUnit(TestRun testRun, string fullTestName, bool isSuite, string testType, string assemblyName)
        {
            Run = testRun;
	        UniqueTestId = fullTestName;
	        IsSuite = isSuite;
	        TestType = testType;
            AssemblyName = assemblyName;
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

        /// <summary>
        /// Gets a value indicating whether this instance is suite.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is suite; otherwise, <c>false</c>.
        /// </value>
	    public bool IsSuite { get; private set; }

        /// <summary>
        /// Gets the type of the test.
        /// </summary>
        /// <value>
        /// The type of the test.
        /// </value>
	    public string TestType { get; private set; }

        /// <summary>
        /// Gets the name of the assembly.
        /// </summary>
        /// <value>
        /// The name of the assembly.
        /// </value>
	    public string AssemblyName { get; private set; }
	}
}