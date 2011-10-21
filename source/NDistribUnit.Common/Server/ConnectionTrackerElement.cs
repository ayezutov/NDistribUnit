using System;
using System.Configuration;

namespace NDistribUnit.Common.Server
{
    /// <summary>
    /// 
    /// </summary>
    public class ConnectionTrackerElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string) base["name"]; }
            set { base["name"] = value; }
        }


        /// <summary>
        /// Gets the type.
        /// </summary>
        public Type Type
        {
            get { return Type.GetType(TypeName); }
        }

        /// <summary>
        /// Gets or sets the name of the type.
        /// </summary>
        /// <value>
        /// The name of the type.
        /// </value>
        [ConfigurationProperty("type", IsRequired = true)]
        public string TypeName
        {
            get { return (string)base["type"]; }
            set { base["type"] = value; }
        }


        /// <summary>
        /// Gets or sets the type of the settings.
        /// </summary>
        /// <value>
        /// The type of the settings.
        /// </value>
        public Type SettingsType
        {
            get { return Type.GetType(SettingsTypeName); }
        }

        /// <summary>
        /// Gets or sets the name of the settings type.
        /// </summary>
        /// <value>
        /// The name of the settings type.
        /// </value>
        [ConfigurationProperty("settingsType", IsRequired = false)]
        public string SettingsTypeName
        {
            get { return (string)base["settingsType"]; }
            set { base["settingsType"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the section.
        /// </summary>
        /// <value>
        /// The name of the section.
        /// </value>
        [ConfigurationProperty("sectionName", IsRequired = false)]
        public string SectionName
        {
            get { return (string)base["sectionName"]; }
            set { base["sectionName"] = value; }
        }
    }
}