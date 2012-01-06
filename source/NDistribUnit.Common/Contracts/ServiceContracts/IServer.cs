using System;
using System.Collections;
using System.ServiceModel;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.ServiceContracts;
using NUnit.Core;

namespace NDistribUnit.Common.Contracts.ServiceContracts
{
    /// <summary>
    /// Contract, which is used by client to connect to server
    /// </summary>
    [ServiceContract(Namespace = "http://yezutov.com/ndistribunit")]
    public interface IServer: IProjectReceiver
    {
        /// <summary>
        /// Runs tests from client
        /// </summary>
        /// <param name="run">The run.</param>
       /// <returns>Test results. It always returns intermediate test results, until the "ndistribunit-completed-merged" property is set</returns>
        [OperationContract]
		TestResult RunTests(TestRun run);

		/// <summary>
		/// Gets the update if available.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <returns></returns>
		[OperationContract]
    	UpdatePackage GetUpdatePackage(Version version);
    }
}