using System;
using NDistribUnit.Common.Contracts.DataContracts;
using NUnit.Core;

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
	    /// <param name="test"></param>
	    /// <param name="testRun">The test run.</param>
	    /// <param name="assemblyName">Name of the assembly.</param>
	    public TestUnit(ITest test, TestRun testRun, string assemblyName)
        {
            Run = testRun;
	        AssemblyName = assemblyName;
	        Info = new TestInfo(test);
        }

        /// <summary>
        /// Gets the test info.
        /// </summary>
	    public TestInfo Info { get; private set; }

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
		public string UniqueTestId { get { return Info.TestName.FullName; } }
        
        /// <summary>
        /// Gets the name of the assembly.
        /// </summary>
        /// <value>
        /// The name of the assembly.
        /// </value>
	    public string AssemblyName { get; private set; }
	}
}