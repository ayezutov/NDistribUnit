using System;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Common.TestExecution.Data
{
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class TestRunRequest: IPingable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestRunRequest"/> class.
		/// </summary>
		/// <param name="testRun">The test run.</param>
		/// <param name="client">The client.</param>
		public TestRunRequest(TestRun testRun, IClient client)
		{
			TestRun = testRun;
			this.client = client;
		    RequestTime = DateTime.UtcNow;
		}

		/// <summary>
		/// Gets or sets the test run.
		/// </summary>
		/// <value>
		/// The test run.
		/// </value>
		public TestRun TestRun { get; private set; }

        [NonSerialized]
	    private IClient client;

	    /// <summary>
		/// Gets the client.
		/// </summary>
		
        public IClient Client
	    {
	        get { return client; }
	    }

	    /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
	    public TestRunRequestStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the request time.
        /// </summary>
        /// <value>
        /// The request time.
        /// </value>
	    public DateTime RequestTime { get; private set; }

        /// <summary>
        /// Gets or sets the configuration setup.
        /// </summary>
        /// <value>
        /// The configuration setup.
        /// </value>
	    public DistributedConfigurationSetup ConfigurationSetup { get; set; }

	    /// <summary>
		/// Pings the tracking side
		/// </summary>
		/// <param name="pingInterval"></param>
		/// <returns>
		/// true if everything is ok
		/// </returns>
		public PingResult Ping(TimeSpan pingInterval)
		{
            if (Client != null)
			    return Client.Ping(pingInterval);
	        return null;
		}

		/// <summary>
		/// Removes the assoiciated client.
		/// </summary>
		public void RemoveClient()
		{
			client = null;
		}

        /// <summary>
        /// Sets the client.
        /// </summary>
        /// <param name="client">The client.</param>
	    public void SetClient(IClient client)
	    {
	        this.client = client;
	    }
	}
}