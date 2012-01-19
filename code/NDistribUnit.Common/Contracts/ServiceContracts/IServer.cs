using System;
using System.Collections;
using System.ServiceModel;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Contracts.DataContracts;
using NUnit.Core;

namespace NDistribUnit.Common.Contracts.ServiceContracts
{
    /// <summary>
    /// Contract, which is used by client to connect to server
    /// </summary>
    [ServiceContract(Namespace = ServiceConfiguration.Namespace)]
    [ServiceKnownType(typeof(ArrayList))]
    [ServiceKnownType(typeof(TestResult))]
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
        /// <param name="request"> </param>
        /// <returns></returns>
        [OperationContract]
    	UpdatePackage GetUpdatePackage(UpdateRequest request);
    }

    /// <summary>
    /// 
    /// </summary>
    [MessageContract]
    public class UpdateRequest
    {
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        [MessageBodyMember]
        public Version Version { get; set; }
    }
}