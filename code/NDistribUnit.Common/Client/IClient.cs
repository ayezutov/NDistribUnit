using System;
using System.Collections;
using System.ServiceModel;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.DataContracts;

namespace NDistribUnit.Common.Client
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceContract(Namespace = ServiceConfiguration.Namespace)]
    [ServiceKnownType(typeof(ArrayList))]
    public interface IClient
    {
        //void TestProgress(TestResult result);

        /// <summary>
        /// Pings the tracking side
        /// </summary>
        /// <param name="pingInterval"></param>
        /// <returns>Anything (including null) if everything is ok, throws exception otherwise</returns>
        [OperationContract]
        PingResult Ping(TimeSpan pingInterval);
    }
}