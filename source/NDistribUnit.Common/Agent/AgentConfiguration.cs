using System;
using System.Configuration;
using NDistribUnit.Common.Common.Logging;

namespace NDistribUnit.Common.Agent
{
    /// <summary>
    ///   The configuration of the NDistribUnit agent
    /// </summary>
    public class AgentConfiguration: ConfigurationSection
    {
        /// <summary>
        ///   Gets or sets the name of the agent.
        /// </summary>
        /// <value>
        ///   The name of the agent.
        /// </value>
        public string AgentName { get; set; }

        /// <summary>
        /// Gets or sets the scope.
        /// </summary>
        /// <value>
        /// The scope.
        /// </value>
        [ConfigurationProperty("scope", IsRequired = true)]
        public Uri Scope
        {
            get { return (Uri)base["scope"]; }
            set { base["scope"] = value; }
        }

        /// <summary>
        ///   Gets or sets the announcement interval.
        /// </summary>
        /// <value>
        ///   The announcement interval.
        /// </value>
        [ConfigurationProperty("announcementIntervalInMilliseconds", IsRequired = true)]
        public TimeSpan AnnouncementInterval
        {
            get { return TimeSpan.FromMilliseconds((int)base["announcementIntervalInMilliseconds"]); }
            set { base["announcementIntervalInMilliseconds"] = value.Milliseconds; }
        }

        /// <summary>
        /// Gets or sets the log configuration.
        /// </summary>
        /// <value>
        /// The log configuration.
        /// </value>
        [ConfigurationProperty("logSettings")]
        public LogConfiguration LogConfiguration 
        {
            get { return (LogConfiguration)base["logSettings"]; }
            set { base["logSettings"] = value; }
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public AgentConfiguration Clone()
        {
            return (AgentConfiguration)MemberwiseClone();
        }
    }
}