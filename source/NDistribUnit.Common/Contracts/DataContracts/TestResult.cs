using System;

namespace NDistribUnit.Common.Contracts.DataContracts
{
    /// <summary>
	/// The result of the test
	/// </summary>
	[Serializable]
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

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is failure.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is failure; otherwise, <c>false</c>.
        /// </value>
        public bool IsFailure
        {
            get { return Exception != null; }
        }
	}
}