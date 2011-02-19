using System.ServiceModel;

namespace NDistribUnit.Common.Communication
{
    [ServiceContract]
    public interface IDuplexService
    {
        [OperationContract(IsOneWay = true)]
        void PerformHeartbeat();
    }
}