using System.ServiceModel;
using System.ServiceModel.Discovery;

namespace NDistribUnit.Common.ServiceContracts
{
    [ServiceContract]
    interface IConnectionTracker
    {
        [OperationContract]
        bool Ping();

        [OperationContract]
        EndpointDiscoveryMetadata GetEndpoint();
    }
}