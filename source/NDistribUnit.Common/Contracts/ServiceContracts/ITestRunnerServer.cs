using System;
using System.ServiceModel;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.DataContracts;

namespace NDistribUnit.Common.ServiceContracts
{
    /// <summary>
    /// Contract, which is used by client to connect to server
    /// </summary>
    [ServiceContract(CallbackContract = typeof(ITestRunnerClient))]
    public interface ITestRunnerServer
    {
    	/// <summary>
    	/// Runs tests from client
    	/// </summary>
    	/// <param name="run"></param>
    	[OperationContract]
		TestRun RunTests(TestRun run);

		/// <summary>
		/// Gets the update if available.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <returns></returns>
		[OperationContract]
    	UpdatePackage GetUpdatePackage(Version version);
    }
}