using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace NDistribUnit.Server
{
    [ServiceContract]
    public interface IDashboardService
    {
        [OperationContract, WebGet(UriTemplate = "get/{*fileName}")]
        Stream Get(string fileName);

        [OperationContract, WebGet(UriTemplate = "getProjectList")]
        ProjectDescription[] GetProjectList();
    }
}