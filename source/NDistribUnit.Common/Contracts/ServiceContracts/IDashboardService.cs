using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Logging;

namespace NDistribUnit.Common.ServiceContracts
{
    /// <summary>
    /// The contract for communicating with dashboard information of the server
    /// It is web-enabled, so is mostly designed to be accessed through browser
    /// </summary>
    [ServiceContract]
    public interface IDashboardService
    {
        /// <summary>
        /// Gets the javascript representation of important enumerations
        /// </summary>
        /// <returns></returns>
        [OperationContract, WebGet(UriTemplate = "get/javascriptEnums.js")]
        Stream GetEnumsJavascriptRegistration();

        /// <summary>
        /// Gets some file from server
        /// </summary>
        /// <param name="fileName">The name of the file to return</param>
        /// <returns></returns>
        [OperationContract, WebGet(UriTemplate = "get/{*fileName}")]
        Stream Get(string fileName);

        /// <summary>
        /// Returns the list of actual projects
        /// </summary>
        /// <returns></returns>
        [OperationContract, WebGet(UriTemplate = "getProjectList")]
        ProjectDescription[] GetProjectList();

        /// <summary>
        /// Gets the statuses of connected agents
        /// </summary>
        /// <returns></returns>
        [OperationContract, WebGet(UriTemplate = "agent/getStatus")]
        AgentInformation[] GetClientStatuses();

        /// <summary>
        /// Gets the log for the server
        /// </summary>
        /// <param name="maxItemsCount">The max items count.</param>
        /// <param name="lastFetchedEntryId">The last fetched entry id.</param>
        /// <returns></returns>
        [OperationContract, WebInvoke(UriTemplate = "server/getLog", BodyStyle = WebMessageBodyStyle.Wrapped)]
        LogEntry[] GetServerLog(int maxItemsCount, int? lastFetchedEntryId = null);

        /// <summary>
        /// Gets the log for the agent
        /// </summary>
        /// <param name="agentName">Name of the agent.</param>
        /// <param name="maxItemsCount">The max items count.</param>
        /// <param name="lastFetchedEntryId">The last fetched entry id.</param>
        /// <returns></returns>
        [OperationContract, WebInvoke(UriTemplate = "agent/getLog", BodyStyle = WebMessageBodyStyle.Wrapped)]
        LogEntry[] GetAgentLog(string agentName, int maxItemsCount, int? lastFetchedEntryId = null);
    }
}