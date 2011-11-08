using System;
using System.ServiceModel;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Server.ConnectionTracking;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.TestExecution.Data;
using NDistribUnit.Common.TestExecution.Storage;
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
        private readonly TestManager manager;
        private readonly RequestsStorage requests;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunnerServer"/> class.
        /// </summary>
        /// <param name="updateSource">The update source.</param>
        /// <param name="versionProvider">The version provider.</param>
        /// <param name="connectionProvider">The connection provider.</param>
        /// <param name="manager">The manager.</param>
        /// <param name="requests">The storage.</param>
    	public TestRunnerServer(IUpdateSource updateSource, 
            IVersionProvider versionProvider, 
            IConnectionProvider connectionProvider,
            TestManager manager,
            RequestsStorage requests)
		{
			this.updateSource = updateSource;
            this.versionProvider = versionProvider;
            this.connectionProvider = connectionProvider;
            this.manager = manager;
            this.requests = requests;
			
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

//            var results = resultsStorage.GetCompletedResults();
//            if (results != null)
//            {
//                client.NotifyTestProgressChanged(results, true);
//                return;
//            }

            var request = requests.AddOrUpdate(run, client);

            if (request.Status == TestRunRequestStatus.Pending)
                client.NotifyTestProgressChanged(null/*resultsStorage.GetIntermediateResults()*/, false);
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
}
