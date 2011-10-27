using System;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Common.TestExecution.Preparation;

namespace NDistribUnit.Common.Server.Services
{
	/// <summary>
	/// 
	/// </summary>
	public class TestRunRequest: IPingable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestRunRequest"/> class.
		/// </summary>
		/// <param name="testRun">The test run.</param>
		/// <param name="client">The client.</param>
		public TestRunRequest(TestRun testRun, ITestRunnerClient client)
		{
			TestRun = testRun;
			Client = client;
		}

		/// <summary>
		/// Gets or sets the test run.
		/// </summary>
		/// <value>
		/// The test run.
		/// </value>
		public TestRun TestRun { get; private set; }

		/// <summary>
		/// Gets the client.
		/// </summary>
		public ITestRunnerClient Client { get; private set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
	    public TestRunRequestStatus Status { get; set; }

	    /// <summary>
		/// Pings the tracking side
		/// </summary>
		/// <param name="pingInterval"></param>
		/// <returns>
		/// true if everything is ok
		/// </returns>
		public PingResult Ping(TimeSpan pingInterval)
		{
			return Client.Ping(pingInterval);
		}

		/// <summary>
		/// Removes the assoiciated client.
		/// </summary>
		public void RemoveClient()
		{
			Client = null;
		}

        /// <summary>
        /// Sets the client.
        /// </summary>
        /// <param name="client">The client.</param>
	    public void SetClient(ITestRunnerClient client)
	    {
	        Client = client;
	    }
	}
}