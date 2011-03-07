using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Server;
using NDistribUnit.Server.Communication;

namespace NDistribUnit.Common.ServiceContracts
{
    [ServiceContract]
    public interface IDashboardService
    {
        [OperationContract, WebGet(UriTemplate = "get/javascriptEnums.js")]
        Stream GetEnumsJavascriptRegistration();

        [OperationContract, WebGet(UriTemplate = "get/{*fileName}")]
        Stream Get(string fileName);

        [OperationContract, WebGet(UriTemplate = "getProjectList")]
        ProjectDescription[] GetProjectList();

        [OperationContract, WebGet(UriTemplate = "getStatus/client")]
        AgentInformation[] GetClientStatuses();
    }
}