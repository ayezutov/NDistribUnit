using System;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Common.Server.Services
{
	/// <summary>
	/// 
	/// </summary>
	public class TestClientDescriptor: IPingable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestClientDescriptor"/> class.
		/// </summary>
		/// <param name="testRun">The test run.</param>
		/// <param name="client">The client.</param>
		public TestClientDescriptor(TestRun testRun, ITestRunnerClient client)
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
	}
}