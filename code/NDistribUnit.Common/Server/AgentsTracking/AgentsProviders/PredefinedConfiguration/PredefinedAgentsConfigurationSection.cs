using System;
using System.Configuration;

namespace NDistribUnit.Common.Server.AgentsTracking.AgentsProviders.PredefinedConfiguration
{
    /// <summary>
    /// Configuration section with agents
    /// </summary>
    public class PredefinedAgentsConfigurationSection: ConfigurationSection
    {
        /// <summary>
        /// Gets the agents.
        /// </summary>
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public PredefinedAgentsElementCollection Agents
        {
            get { return (PredefinedAgentsElementCollection)base[""]; }
        }

        /// <summary>
        /// Gets the recheck interval.
        /// </summary>
        [ConfigurationProperty("recheckInterval", IsDefaultCollection = true, DefaultValue = "0:0:15")]
        public TimeSpan RecheckInterval
        {
            get { return (TimeSpan)base["recheckInterval"]; }
        }
    }
}