using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using NDistribUnit.Common.Agent;
using System.Linq;

namespace NDistribUnit.Common.Common.Communication
{
    /// <summary>
    /// Performs all operations, which are related to managing additional information passed from the agent
    /// </summary>
    public static class AdditionalDataManager
    {
        private const string infoElementName = "info";
        private const string nameElementName = "name";
        private const string versionElementName = "version";

        /// <summary>
        /// Gets the additional information.
        /// </summary>
        /// <param name="agentHost">The agent host.</param>
        /// <returns></returns>
        private static XElement GetAdditionalInformation(AgentHost agentHost)
         {
             return new XElement(infoElementName, 
				 new XElement(nameElementName, agentHost.TestRunner.Name),
				 new XElement(versionElementName, agentHost.TestRunner.Version.ToString()));
         }

        /// <summary>
        /// Adds the specified host's information to extensions.
        /// </summary>
        /// <param name="extensions">The extensions.</param>
        /// <param name="host">The host.</param>
        public static void Add(Collection<XElement> extensions, AgentHost host)
        {
            if (extensions.FirstOrDefault(e => e.Name == infoElementName) == null)
                extensions.Add(GetAdditionalInformation(host));
        }

        /// <summary>
        /// Gets the name of the agent.
        /// </summary>
        /// <param name="extensions">The extensions.</param>
        /// <returns></returns>
        public static string GetName(IEnumerable<XElement> extensions)
        {
        	return GetInfoNode(extensions, nameElementName);
        }

		/// <summary>
		/// Gets the agent version.
		/// </summary>
		/// <param name="extensions">The extensions.</param>
		/// <returns></returns>
		public static Version GetVersion(IEnumerable<XElement> extensions)
		{
			var version = GetInfoNode(extensions, versionElementName);
			return version == null ? null : new Version(version);
		}

    	private static string GetInfoNode(IEnumerable<XElement> extensions, string elementName)
    	{
    		XElement infoElement = extensions.FirstOrDefault(e => e.Name == infoElementName);

    		if (infoElement == null)
    			return null;

    		var element = infoElement.Element(XName.Get(elementName));

    		if (element == null)
    			return null;

    		return element.Value;
    	}
    }
}