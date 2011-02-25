using System.ServiceModel;

namespace NDistribUnit.Common.ServiceContracts
{
    [ServiceContract]
    public interface ITestRunnerServer
    {
        [OperationContract]
        void RunTests();
    }
}