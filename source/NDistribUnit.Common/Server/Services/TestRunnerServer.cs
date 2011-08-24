using System;
using System.Reflection;
using System.ServiceModel;
using NDistribUnit.Common;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Server.Services
{
    /// <summary>
    /// The test runner service, which is used by clients to tell the job
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class TestRunnerServer : ITestRunnerServer
    {
    	private readonly IUpdateSource updateSource;
    	private string g = Guid.NewGuid().ToString();

		/// <summary>
		/// Initializes a new instance of the <see cref="TestRunnerServer"/> class.
		/// </summary>
		/// <param name="updateSource">The update source.</param>
    	public TestRunnerServer(IUpdateSource updateSource)
		{
			this.updateSource = updateSource;
		}

    	/// <summary>
        /// Runs tests from client
        /// </summary>
        public void RunTests()
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
    		var currentVersion = Assembly.GetEntryAssembly().GetName().Version;
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
