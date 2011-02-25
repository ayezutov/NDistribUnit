using System.ServiceModel;
using NDistribUnit.Common.Communication;

namespace NDistribUnit.Common.ServiceContracts
{
    [ServiceContract]
    public interface ITestRunnerAgent: IPingable
    {
        [OperationContract]
        bool RunTests(string callbackValue);
    }
}