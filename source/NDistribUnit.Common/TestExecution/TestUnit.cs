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
		/// Gets or sets the test run.
		/// </summary>
		/// <value>
		/// The run.
		/// </value>
		public TestRun Run { get; set; }

		/// <summary>
		/// Gets or sets the unique test id.
		/// </summary>
		/// <value>
		/// The unique test id.
		/// </value>
		public string UniqueTestId { get; set; }
	}
}