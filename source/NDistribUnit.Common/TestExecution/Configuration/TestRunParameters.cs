using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
            return SpecialHandlings.FirstOrDefault(h =>
                                                       {
                                                           if (h.FailureMessageType == MatchType.ContainsText)
                                                               return exception.Message.Contains(h.FailureMessage) 
                                                                   || exception.StackTrace.Contains(h.FailureMessage);
                                                            if (h.FailureMessageType == MatchType.Regex)
                                                            {
                                                                var regex = new Regex(h.FailureMessage);
                                                                return regex.IsMatch(exception.Message) 
                                                                    || regex.IsMatch(exception.StackTrace);
                                                            }

                                                           return false;
                                                       });
        }
    }
}