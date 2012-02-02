using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

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

        /// <summary>
        /// Gets the special handling.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public TestRunFailureSpecialHandling GetSpecialHandling(Exception exception)
        {
            return GetSpecialHandling(exception.Message, exception.StackTrace);
        }

        /// <summary>
        /// Gets the special handling.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="stackTrace">The stack trace.</param>
        public TestRunFailureSpecialHandling GetSpecialHandling(string message, string stackTrace)
        {
            return SpecialHandlings.FirstOrDefault(h => h.IsMatching(message, stackTrace));
        }
    }
}