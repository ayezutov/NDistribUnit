using System;
using System.Collections;
using System.ServiceModel;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Common.Contracts.ServiceContracts
{
    /// <summary>
    /// Contract, which is used by client to connect to server
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IClient))]
    public interface IServer
    {
        /// <summary>
        /// Runs tests from client
        /// </summary>
        /// <param name="run"></param>
        [OperationContract]
		void StartRunningTests(TestRun run);

		/// <summary>
		/// Gets the update if available.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <returns></returns>
		[OperationContract]
    	UpdatePackage GetUpdatePackage(Version version);
    }
}