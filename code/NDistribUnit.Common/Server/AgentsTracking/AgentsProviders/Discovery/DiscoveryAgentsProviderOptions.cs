using System.Configuration;

namespace NDistribUnit.Common.Server.AgentsTracking.AgentsProviders.Discovery
{
    /// <summary>
    /// The options for <see cref="DiscoveryAgentsProvider"/>
    /// </summary>
    public class DiscoveryAgentsProviderOptions: ConfigurationSection
    {
        /// <summary>
        /// Gets or sets the discovery interval in miliseconds.
        /// </summary>
        /// <value>
        /// The discovery interval in miliseconds.
        /// </value>
        [ConfigurationProperty("discoveryIntervalInMilliseconds", DefaultValue = 20000, IsRequired = true)]
        [IntegerValidator(MinValue = 500, MaxValue = 3600000)]
        public int DiscoveryIntervalInMiliseconds 
        {
            get { return (int) base["discoveryIntervalInMilliseconds"]; }
            set { base["discoveryIntervalInMilliseconds"] = value; }
        }
    }
}