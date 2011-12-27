using System;
using System.ServiceModel;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.Logging;
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
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, 
        InstanceContextMode = InstanceContextMode.Single,
        MaxItemsInObjectGraph = 2147483647)]
    [CallbackBehavior(MaxItemsInObjectGraph = 2147483647)]
    public class Server : IServer
    {
    	private readonly IUpdateSource updateSource;
        private readonly IVersionProvider versionProvider;
        private readonly IConnectionProvider connectionProvider;
        private readonly ServerTestRunner runner;
        private readonly IRequestsStorage requests;
        private readonly IResultsStorage resultsStorage;
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="updateSource">The update source.</param>
        /// <param name="versionProvider">The version provider.</param>
        /// <param name="connectionProvider">The connection provider.</param>
        /// <param name="runner">The manager.</param>
        /// <param name="requests">The storage.</param>
        /// <param name="resultsStorage">The results.</param>
        /// <param name="log">The log.</param>
    	public Server(
            IUpdateSource updateSource, 
            IVersionProvider versionProvider, 
            IConnectionProvider connectionProvider,
            ServerTestRunner runner,
            IRequestsStorage requests,
            IResultsStorage resultsStorage,
            ILog log)
		{
			this.updateSource = updateSource;
            this.versionProvider = versionProvider;
            this.connectionProvider = connectionProvider;
            this.runner = runner;
            this.requests = requests;
            this.resultsStorage = resultsStorage;
            this.log = log;
		}
        
        /// <summary>
        /// Runs tests from client
        /// </summary>
        /// <param name="run"></param>
        public void StartRunningTests(TestRun run)
    	{
            log.BeginActivity(string.Format("Test was started: {0} ({1})", run.Id, run.NUnitParameters.AssembliesToTest[0]));
    	    var client = connectionProvider.GetCurrentCallback<IClient>();

            if (run == null)
                throw new ArgumentNullException("run");

            var result = resultsStorage.GetCompletedResult(run);
            if (result != null)
            {
                client.NotifyTestProgressChanged(result, true);
                return;
            }

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
