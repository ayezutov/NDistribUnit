using System;

namespace NDistribUnit.Common.Client
{
    /// <summary>
	/// The result of the test
	/// </summary>
	public class TestResult
	{
        private readonly string message;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestResult"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
		public TestResult(string message)
		{
		    this.message = message;
		}
	}
}