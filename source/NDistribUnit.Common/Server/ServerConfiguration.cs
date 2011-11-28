using System;
using System.Configuration;
using NDistribUnit.Common.Common.Logging;
using NDistribUnit.Common.Server.ConnectionTracking;

namespace NDistribUnit.Common.Server
{
    /// <summary>
    /// Parsed server parameters
    /// </summary>
    public class ServerConfiguration : ConfigurationSection, IConnectionsHostOptions
    {
        /// <summary>
        /// Gets the dashboard port.
        /// </summary>
        [ConfigurationProperty("dashboardPort", DefaultValue = 8008, IsRequired = true)]
        [IntegerValidator(MinValue = 0, MaxValue = 65536)]
        public int DashboardPort
        {
            get { return (int)base["dashboardPort"]; }
            set { base["dashboardPort"] = value; }
        }

        /// <summary>
        /// Gets the test runner port.
        /// </summary>
        [ConfigurationProperty("testRunnerPort", DefaultValue = 8009, IsRequired = true)]
        [IntegerValidator(MinValue = 0, MaxValue = 65536)]
        public int TestRunnerPort
        {
            get { return (int)base["testRunnerPort"]; }
            set { base["testRunnerPort"] = value; }
        }

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
        /// Gets or sets the ping interval in miliseconds.
        /// </summary>
        /// <value>
        /// The ping interval in miliseconds.
        /// </value>
        [ConfigurationProperty("pingIntervalInMiliseconds", DefaultValue = 5000, IsRequired = true)]
        [IntegerValidator(MinValue = 500, MaxValue = 3600000)]
        public int PingIntervalInMiliseconds
        {
            get { return (int)base["pingIntervalInMiliseconds"]; }
            set { base["pingIntervalInMiliseconds"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [use discovery tracker].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use discovery tracker]; otherwise, <c>false</c>.
        /// </value>
        public bool UseDiscoveryTracker { get; set; }

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
        /// Gets or sets the connection trackers.
        /// </summary>
        /// <value>
        /// The connection trackers.
        /// </value>
        [ConfigurationProperty("connectionTrackers")]
        public ConnectionTrackerConfigurations ConnectionTrackers
        {
            get { return (ConnectionTrackerConfigurations) base["connectionTrackers"]; }
            set { base["connectionTrackers"] = value; }
        }
    }

    /// <summary>
    /// Represents the list of connection trackers
    /// </summary>
    public class ConnectionTrackerConfigurations: ConfigurationElementCollection
    {
        /// <summary>
        /// When overridden in a derived class, creates a new <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new ConnectionTrackerElement();
        }

        /// <summary>
        /// Gets the element key for a specified configuration element when overridden in a derived class.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement"/> to return the key for.</param>
        /// <returns>
        /// An <see cref="T:System.Object"/> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ConnectionTrackerElement) element).Name;
        }


    }
}