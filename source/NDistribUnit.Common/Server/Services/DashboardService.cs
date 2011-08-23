using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Server.Communication;
using System.Linq;

namespace NDistribUnit.Server.Services
{
    /// <summary>
    /// The service, which allows a web-based status tracking of the server
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class DashboardService : IDashboardService
    {
        private readonly ServerConnectionsTracker connectionsTracker;
        private readonly RollingLog log;

        private static readonly IDictionary<string, string> allowed =
            new Dictionary<string, string>
                {
                    {".html", "text/html"},
                    {".htm", "text/html"},
                    {".js", "text/javascript"},
                    {".css", "text/css"},
                    {".png", "image/png"},
                    {".gif", "image/gif"},
                    {".jpg", "image/jpeg"}
                };

        /// <summary>
        /// Initializes a new instance of dashboard service
        /// </summary>
        /// <param name="connectionsTracker">The <see cref="ServerConnectionsTracker">connections tracker</see> for the server</param>
        /// <param name="log">The log to display for requests</param>
        public DashboardService(ServerConnectionsTracker connectionsTracker, RollingLog log)
        {
            this.connectionsTracker = connectionsTracker;
            this.log = log;
        }

        /// <summary>
        /// Returns the list of actual projects
        /// </summary>
        /// <returns></returns>
        public ProjectDescription[] GetProjectList()
        {
            return new []
                       {
                           new ProjectDescription
                               {
                                   Name = "O2I Nightly",
                                   UniqueIdentifier = Guid.NewGuid().ToString()
                               },
                           new ProjectDescription
                               {
                                   Name = "O2I Build for 6.0",
                                   UniqueIdentifier = Guid.NewGuid().ToString()
                               },
                           new ProjectDescription
                               {
                                   Name = "O2I Build for 5.2",
                                   UniqueIdentifier = Guid.NewGuid().ToString()
                               },
                           new ProjectDescription
                               {
                                   Name = "BDB Nightly",
                                   UniqueIdentifier = Guid.NewGuid().ToString()
                               },
                       };
        }

        /// <summary>
        /// Gets the statuses of connected agents
        /// </summary>
        /// <returns></returns>
        public AgentInformation[] GetClientStatuses()
        {
            return connectionsTracker.Agents.ToArray();
        }

        /// <summary>
        /// Gets the log for the server
        /// </summary>
        /// <returns></returns>
        public LogEntry[] GetServerLog(int maxItemsCount, int? lastFetchedEntryId = null)
        {
            return log.GetEntries(lastFetchedEntryId, maxItemsCount);
        }

        /// <summary>
        /// Gets the log for the agent
        /// </summary>
        /// <param name="agentName">Name of the agent.</param>
        /// <param name="maxItemsCount">The max items count.</param>
        /// <param name="lastFetchedEntryId">The last fetched entry id.</param>
        /// <returns></returns>
        public LogEntry[] GetAgentLog(string agentName, int maxItemsCount, int? lastFetchedEntryId = null)
        {
            AgentInformation agent = connectionsTracker.Agents.FirstOrDefault(a => a.Name.Equals(agentName));

            if (agent == null)
                return new LogEntry[0];

            LogEntry[] result = agent.GetNetTcpChannel<ITestRunnerAgent>()
                .GetLog(maxItemsCount, lastFetchedEntryId);
            return result;
        }

        private static string enumsJavascriptRegistration;

        /// <summary>
        /// Gets the javascript representation of important enumerations
        /// </summary>
        /// <returns></returns>
        public Stream GetEnumsJavascriptRegistration()
        {
            Debug.Assert(WebOperationContext.Current != null);
            WebOperationContext.Current.OutgoingResponse.ContentType = allowed[".js"];
            if (string.IsNullOrEmpty(enumsJavascriptRegistration))
            {
                var enumsToRegister = new[]
                                             {
                                                 typeof(AgentState), 
                                                 typeof(LogEntryType)
                                             };
                var sb = new StringBuilder();
                foreach (var type in enumsToRegister)
                {
                    sb.AppendFormat("var {0} = {{", type.Name);
                    bool first = true;
                    foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
                    {
                        if(!first)
                            sb.Append(",");
                        first = false;

                        sb.AppendFormat("{0}: {1}", field.Name, field.GetRawConstantValue());
                    }
                    sb.Append("}; ");
                }
                enumsJavascriptRegistration = sb.ToString();
            }
            return new MemoryStream(Encoding.UTF8.GetBytes(enumsJavascriptRegistration));
        }

        /// <summary>
        /// Gets some file from server
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Stream Get(string fileName)
        {
            Debug.Assert(WebOperationContext.Current != null);

            var physicalPathToFile =
                Path.Combine(
                    Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetEntryAssembly().CodeBase).AbsolutePath),
                                 "dashboard"), fileName);
            var response = WebOperationContext.Current.OutgoingResponse;

            if (!File.Exists(physicalPathToFile))
            {
                response.SetStatusAsNotFound();
                return new MemoryStream(Encoding.UTF8.GetBytes("Not found"));
            }

            var extension = Path.GetExtension(physicalPathToFile);
            if (!IsAllowedExtension(extension) || extension == null)
            {
                response.StatusCode = HttpStatusCode.Forbidden;
                return new MemoryStream(Encoding.UTF8.GetBytes("Forbidden"));
            }

            response.ContentType = allowed[extension];
            return new FileStream(physicalPathToFile, FileMode.Open, FileAccess.Read);
        }

        private static bool IsAllowedExtension(string extension)
        {
            return allowed.Keys.Contains(extension);
        }
    }
}