using System.ServiceModel;

namespace NDistribUnit.Common.Communication
{
    [ServiceContract]
    public interface IPingable
    {
        [OperationContract]
        bool Ping();
    }
}