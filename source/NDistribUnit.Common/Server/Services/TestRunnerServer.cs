using System;
using System.Collections.Generic;
using System.ServiceModel;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Server.ConnectionTracking;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Common.TestExecution.Preparation;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Common.Server.Services
{
    /// <summary>
    /// The test runner service, which is used by clients to tell the job
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class TestRunnerServer : ITestRunnerServer
    {
    	private readonly IUpdateSource updateSource;
        private readonly IVersionProvider versionProvider;
        private readonly IConnectionProvider connectionProvider;
        private readonly TestRunRequestsStorage requests;
        private readonly PingableCollection<TestRunRequest> clients;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunnerServer"/> class.
        /// </summary>
        /// <param name="updateSource">The update source.</param>
        /// <param name="options">The options.</param>
        /// <param name="versionProvider">The version provider.</param>
        /// <param name="connectionProvider">The connection provider.</param>
        /// <param name="requests">The storage.</param>
    	public TestRunnerServer(IUpdateSource updateSource, 
            IConnectionsHostOptions options, 
            IVersionProvider versionProvider, 
            IConnectionProvider connectionProvider,
            TestRunRequestsStorage requests)
		{
			this.updateSource = updateSource;
            this.versionProvider = versionProvider;
            this.connectionProvider = connectionProvider;
            this.requests = requests;
            clients = new PingableCollection<TestRunRequest>(options);
			clients.Removed += OnClientRemoved;
		}

    	private void OnClientRemoved(object sender, EventArgs<TestRunRequest> e)
    	{
    		e.Data.RemoveClient();
    	}

        /// <summary>
        /// Runs tests from client
        /// </summary>
        /// <param name="run"></param>
        public void StartRunningTests(TestRun run)
    	{
    	    var client = connectionProvider.GetCurrentCallback<ITestRunnerClient>();

            if (run == null)
                throw new ArgumentNullException("run");

            #region Stub for the future
            //          var runState = GetStateBytestRun();
            //          if (runState != null){

            //			if (runState.IsCompleted(run))
            //			{
            //				client.NotifyTestCompleted(runState.GetResults(run.Id), );
            //				return run;
            //			}
            //    		
            //			if (runState.IsPending(run))
            //    		{ 
            //    			clientRunners.Replace(new TestClient(savedRun, client));
            //    			return run;
            //    		}
            //          }
            #endregion

            var request = new TestRunRequest(run, client);
			clients.Add(request);
            requests.Add(request);
    	}

    	/// <summary>
		/// Gets the update if available.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <returns></returns>
    	public UpdatePackage GetUpdatePackage(Version version)
    	{
    		var currentVersion = versionProvider.GetVersion();
    		if (version >= currentVersion)
    			return new UpdatePackage
    			       	{
    			       		IsAvailable = false
    			       	};

			return new UpdatePackage
			       	{
			       		IsAvailable = true,
						UpdateZipBytes = updateSource.GetZippedVersionFolder(currentVersion),
						Version = currentVersion
			       	};
    	}
    }

    /// <summary>
    /// 
    /// </summary>
    public class TestsToRun
    {
        /// <summary>
        /// Gets or sets the categories to include.
        /// </summary>
        /// <value>
        /// The include categories.
        /// </value>
        public IList<string> IncludeCategories { get; set; }

        /// <summary>
        /// Gets or sets the categories to include.
        /// </summary>
        /// <value>
        /// The include categories.
        /// </value>
        public IList<string> ExcludeCategories { get; set; }
    }
}
