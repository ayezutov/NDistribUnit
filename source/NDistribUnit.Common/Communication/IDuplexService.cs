using System.ServiceModel;

namespace NDistribUnit.Common.Communication
{
    public interface IDuplexService
    {
        [OperationContract(IsOneWay = true)]
        void CallHeartbeatMethod();
    }
}