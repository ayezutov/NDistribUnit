using System.Configuration;

namespace NDistribUnit.Common.Common.Logging
{
    /// <summary>
    /// 
    /// </summary>
    public class LogConfiguration: ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the rolling log items count.
        /// </summary>
        /// <value>
        /// The rolling log items count.
        /// </value>
        [ConfigurationProperty("rollingLogItemsCount", DefaultValue = 1000, IsRequired = true)]
        [IntegerValidator(MinValue = 100, MaxValue = int.MaxValue)]
        public int RollingLogItemsCount
        {
            get { return (int)base["rollingLogItemsCount"]; }
            set { base["rollingLogItemsCount"] = value; }
        }
    }
}