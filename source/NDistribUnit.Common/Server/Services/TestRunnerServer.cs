using System;
using System.Reflection;
using System.ServiceModel;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.Communication.ConnectionTracking;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Server.ConnectionTracking;
using NDistribUnit.Common.ServiceContracts;
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
        private readonly PingableCollection<TestClientDescriptor> clientRunners;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunnerServer"/> class.
        /// </summary>
        /// <param name="updateSource">The update source.</param>
        /// <param name="options">The options.</param>
        /// <param name="versionProvider">The version provider.</param>
    	public TestRunnerServer(IUpdateSource updateSource, IConnectionsHostOptions options, IVersionProvider versionProvider)
		{
			this.updateSource = updateSource;
            this.versionProvider = versionProvider;
            clientRunners = new PingableCollection<TestClientDescriptor>(options);
			clientRunners.Removed += OnClientRemoved;
		}

    	private void OnClientRemoved(object sender, EventArgs<TestClientDescriptor> e)
    	{
    		e.Data.RemoveClient();
    	}

    	/// <summary>
    	/// Runs tests from client
    	/// </summary>
    	/// <param name="run"></param>
    	public TestRun RunTests(TestRun run)
    	{
			var client = OperationContext.Current.GetCallbackChannel<ITestRunnerClient>();

            if (run == null)
			{
				run = new TestRun { Id = Guid.NewGuid() };
				var testClient = new TestClientDescriptor(run, client);
				clientRunners.Add(testClient);
				StartTestRun(testClient);
				return run;
			}
			
//			if (runState.IsCompleted(run))
//			{
//				client.NotifyTestCompleted(runState.GetResults(run.Id));
//				return run;
//			}
//    		
//			if (runState.IsPending(run))
//    		{ 
//    			clientRunners.Replace(new TestClient(savedRun, client));
//    			return run;
//    		}

			client.NotifyTestCompleted(new TestResult("No test was found with the id={0}"));
    		return run;
    	}

    	private void StartTestRun(TestClientDescriptor testClientDescriptor)
    	{
    		throw new NotImplementedException();
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
