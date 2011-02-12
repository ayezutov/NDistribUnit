using System.ServiceModel;

namespace NDistribUnit.Common
{
    public interface ICallbacks
    {
        [OperationContract]
        bool MyCallbackFunction(string callbackValue);
    }
}