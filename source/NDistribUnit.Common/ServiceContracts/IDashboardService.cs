﻿using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;
using NDistribUnit.Common.DataContracts;

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
        /// <param name="fileName"></param>
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
        [OperationContract, WebGet(UriTemplate = "getStatus/client")]
        AgentInformation[] GetClientStatuses();
    }
}