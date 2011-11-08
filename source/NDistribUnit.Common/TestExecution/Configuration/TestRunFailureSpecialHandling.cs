using System.Xml.Serialization;

namespace NDistribUnit.Common.TestExecution.Configuration
{
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
}