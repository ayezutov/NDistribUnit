using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace NDistribUnit.Common.TestExecution.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    [XmlRoot("ndistribunit-project")]
    [Serializable]
    public class TestRunParameters
    {
        /// <summary>
        /// Gets the default.
        /// </summary>
        public static TestRunParameters Default =  new TestRunParameters
                                                       {
                           SpecialHandlings = new List<TestRunFailureSpecialHandling>(),
                           MaximumAgentsCount = Int32.MaxValue
                       };

        /// <summary>
        /// Gets or sets the special handlings.
        /// </summary>
        /// <value>
        /// The special handlings.
        /// </value>
        [XmlArray("specialFailureHandling")]
        [XmlArrayItem("failure")]
        public List<TestRunFailureSpecialHandling> SpecialHandlings { get; set; }

        /// <summary>
        /// Gets or sets the maximum agents count.
        /// </summary>
        /// <value>
        /// The maximum agents count.
        /// </value>
        [XmlElement("maximumAgentsCount")]
        public int MaximumAgentsCount { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class TestRunFailureSpecialHandling
    {
        /// <summary>
        /// Gets or sets the failure message.
        /// </summary>
        /// <value>
        /// The failure message.
        /// </value>
        [XmlAttribute("message")]
        public string FailureMessage { get; set; }

        /// <summary>
        /// Gets or sets the type of the failure message.
        /// </summary>
        /// <value>
        /// The type of the failure message.
        /// </value>
        [XmlAttribute("messageType")]
        public FailureMessageType FailureMessageType { get; set; }

        /// <summary>
        /// Gets or sets the retry count.
        /// </summary>
        /// <value>
        /// The retry count.
        /// </value>
        [XmlAttribute("retryCount")]
        public int RetryCount { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum FailureMessageType
    {
        /// 
        ContainsText,
        /// 
        Regex
    }
}