using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using NDistribUnit.Common.Agent;
using System.Linq;

namespace NDistribUnit.Common.Communication.ConnectionTracking
{
    /// <summary>
    /// Performs all operations, which are related to managing additional information passed from the agent
    /// </summary>
    public static class AgentAdditionalDataManager
    {
        private const string agentInfoElementName = "agentInfo";
        private const string agentNameElementName = "agentName";

        /// <summary>
        /// Gets the additional information.
        /// </summary>
        /// <param name="agentHost">The agent host.</param>
        /// <returns></returns>
        private static XElement GetAdditionalInformation(AgentHost agentHost)
         {
             return new XElement(agentInfoElementName, new XElement(agentNameElementName, agentHost.TestRunner.Name));
         }

        /// <summary>
        /// Adds the specified host's information to extensions.
        /// </summary>
        /// <param name="extensions">The extensions.</param>
        /// <param name="host">The host.</param>
        public static void Add(Collection<XElement> extensions, AgentHost host)
        {
            if (extensions.FirstOrDefault(e => e.Name == agentInfoElementName) == null)
                extensions.Add(GetAdditionalInformation(host));
        }

        /// <summary>
        /// Gets the name of the agent.
        /// </summary>
        /// <param name="extensions">The extensions.</param>
        /// <returns></returns>
        public static string GetAgentName(IEnumerable<XElement> extensions)
        {
            XElement infoElement = extensions.FirstOrDefault(e => e.Name == agentInfoElementName);

            if (infoElement == null)
                return null;

            var agentNameElement = infoElement.Element(XName.Get(agentNameElementName));

            if(agentNameElement == null)
                return null;

            return agentNameElement.Value;
        }
    }
}