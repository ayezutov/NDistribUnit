using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
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
        private const string versionElementName = "agentVersion";

        /// <summary>
        /// Gets the additional information.
        /// </summary>
        /// <param name="agentHost">The agent host.</param>
        /// <returns></returns>
        private static XElement GetAdditionalInformation(AgentHost agentHost)
         {
             return new XElement(agentInfoElementName, 
				 new XElement(agentNameElementName, agentHost.TestRunner.Name),
				 new XElement(versionElementName, Assembly.GetEntryAssembly().GetName().Version.ToString()));
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
        	return GetAgentInfoNode(extensions, agentNameElementName);
        }

		/// <summary>
		/// Gets the agent version.
		/// </summary>
		/// <param name="extensions">The extensions.</param>
		/// <returns></returns>
		public static Version GetAgentVersion(IEnumerable<XElement> extensions)
		{
			var version = GetAgentInfoNode(extensions, versionElementName);
			return version == null ? null : new Version(version);
		}

    	private static string GetAgentInfoNode(IEnumerable<XElement> extensions, string elementName)
    	{
    		XElement infoElement = extensions.FirstOrDefault(e => e.Name == agentInfoElementName);

    		if (infoElement == null)
    			return null;

    		var element = infoElement.Element(XName.Get(elementName));

    		if (element == null)
    			return null;

    		return element.Value;
    	}
    }
}