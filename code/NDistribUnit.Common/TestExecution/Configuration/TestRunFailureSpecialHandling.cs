using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace NDistribUnit.Common.TestExecution.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
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
        public MatchType FailureMessageType { get; set; }

        /// <summary>
        /// Gets or sets the retry count.
        /// </summary>
        /// <value>
        /// The retry count.
        /// </value>
        [XmlAttribute("retryCount")]
        public int RetryCount { get; set; }

        /// <summary>
        /// Gets or sets the stack trace.
        /// </summary>
        /// <value>
        /// The stack trace.
        /// </value>
        [XmlAttribute("stackTrace")]
        public string FailureStackTrace { get; set; }

        /// <summary>
        /// Gets or sets the type of the failure stack trace.
        /// </summary>
        /// <value>
        /// The type of the failure stack trace.
        /// </value>
        [XmlAttribute("stackTraceType")]
        public MatchType FailureStackTraceType { get; set; }

        /// <summary>
        /// Determines whether the specified message is matching.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="stackTrace">The stack trace.</param>
        /// <returns>
        ///   <c>true</c> if the specified message is matching; otherwise, <c>false</c>.
        /// </returns>
        public bool IsMatching(string message, string stackTrace)
        {
            Initialize();

            if (isMessageMatching == null && isStackTraceMatching == null)
                return false;

            if (isStackTraceMatching != null && isMessageMatching != null)
                return isMessageMatching(message) && isStackTraceMatching(stackTrace);

            if (isMessageMatching != null)
                return isMessageMatching(message);

            if (isStackTraceMatching != null)
                return isStackTraceMatching(stackTrace);

            return false;
        }

        private void Initialize()
        {
            if (initialized)
                return;
            lock(this)
            {
                if (initialized)
                    return;

                isMessageMatching = GetMatchingFunction(FailureMessage, FailureMessageType);
                isStackTraceMatching = GetMatchingFunction(FailureStackTrace, FailureStackTraceType);

                initialized = true;
            }
        }

        private Func<string, bool> GetMatchingFunction(string value, MatchType matchingType)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (matchingType == MatchType.Regex)
            {
                var regex = new Regex(value, RegexOptions.Compiled);
                return s => s != null && regex.IsMatch(s);
            }

            return s => s != null && s.Contains(value);
        }

        [NonSerialized]
        private bool initialized;
        [NonSerialized]
        private Func<string, bool> isMessageMatching;
        [NonSerialized]
        private Func<string, bool> isStackTraceMatching;
    }
}