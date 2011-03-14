using System;

namespace NDistribUnit.Common.Logging
{
    /// <summary>
    /// The entry, which is saved in log
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// The uniquely assigned id for some entry
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// The message, which is saved in log
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// The exception (if any), which is saved in log
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// The type of the log entry
        /// </summary>
        public LogEntryType Type { get; private set; }

        /// <summary>
        /// Value, specifying, whether the entry has an exception attached.
        /// </summary>
        public bool HasException
        {
            get { return Exception != null; }
        }
    }
}