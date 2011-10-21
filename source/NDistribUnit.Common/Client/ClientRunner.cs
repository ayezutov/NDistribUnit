using System;
using System.ServiceModel;
using NDistribUnit.Client.Configuration;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Common.Client
{
	/// <summary>
	/// 
	/// </summary>
	public class TestRunnerClient: ITestRunnerClient
	{
		private readonly ClientParameters options;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestRunnerClient"/> class.
		/// </summary>
		/// <param name="options">The options.</param>
		/// <param name="updateReceiver">The update receiver.</param>
		public TestRunnerClient(ClientParameters options, IUpdateReceiver updateReceiver)
		{
			this.options = options;
		}

		/// <summary>
		/// Notifies that the test has completed.
		/// </summary>
		/// <param name="result">The result.</param>
		public void NotifyTestCompleted(TestResult result)
		{
			
		}

		/// <summary>
		/// Pings the tracking side
		/// </summary>
		/// <param name="pingInterval"></param>
		/// <returns>
		/// Anything (including null) if everything is ok, throws exception otherwise
		/// </returns>
		public PingResult Ping(TimeSpan pingInterval)
		{
			return null;
		}

		/// <summary>
		/// Runs this instance.
		/// </summary>
		public void Run()
		{
			TestRun run = null; //TODO: load saved state here
			var server = DuplexChannelFactory<ITestRunnerServer>.CreateChannel(this,
			                                                      new NetTcpBinding("NDistribUnit.Default"),
			                                                      new EndpointAddress(options.ServerUri));
			run = server.RunTests(run);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	[ServiceContract]
	public interface ITestRunnerClient: IPingable
	{
		//void TestProgress(TestResult result);

		/// <summary>
		/// Notifies that the test has completed.
		/// </summary>
		/// <param name="result">The result.</param>
		void NotifyTestCompleted(TestResult result);
	}

	/// <summary>
	/// The result of the test
	/// </summary>
	public class TestResult
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestResult"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public TestResult(string message)
		{
			throw new NotImplementedException();
		}
	}
}