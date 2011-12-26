using System.Configuration;

namespace NDistribUnit.Common.Server.AgentsTracking.AgentsProviders.PredefinedConfiguration
{
    /// <summary>
    /// Single agents element in configuration section
    /// </summary>
    public class PredefinedAgentsElement: ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        [ConfigurationProperty("url", IsKey = true, IsRequired = true)]
        public string Url
        {
            get { return (string)base["url"]; }
            set { base["url"] = value; }
        }
    }
}