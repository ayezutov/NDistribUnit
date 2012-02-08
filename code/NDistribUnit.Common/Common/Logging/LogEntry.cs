using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using NDistribUnit.Common.Logging;

namespace NDistribUnit.Common.Common.Logging
{
    /// <summary>
    /// The entry, which is saved in log
    /// </summary>
    [DebuggerDisplay("{Type}: {Message}")]
    [DataContract]
    public class LogEntry
    {
        /// <summary>
        /// Initializes a new instance of a log entry
        /// </summary>
        /// <param name="id">The unique id for this entry</param>
        /// <param name="type">The log entry type</param>
        /// <param name="message">The message to be logged</param>
        /// <param name="logTime">The log time.</param>
        /// <param name="exception">The exception information to be logged</param>
        public LogEntry(int id, LogEntryType type, string message, DateTime logTime, Exception exception = null)
        {
            Id = id;
            Type = type;
            Message = message;
            Exception = exception != null ? new ExceptionEntry(exception) : null;
            LogTime = logTime;
        }

        /// <summary>
        /// The uniquely assigned id for some entry
        /// </summary>
        [DataMember]
        public int Id { get; private set; }

        /// <summary>
        /// The message, which is saved in log
        /// </summary>
        [DataMember]
        public string Message { get; private set; }

        /// <summary>
        /// The exception (if any), which is saved in log
        /// </summary>
        [DataMember]
        public ExceptionEntry Exception { get; private set; }

        /// <summary>
        /// The type of the log entry
        /// </summary>
        [DataMember]
        public LogEntryType Type { get; private set; }

        /// <summary>
        /// Gets the log time.
        /// </summary>
        [DataMember]
        public DateTime LogTime { get; private set; }
    }
}