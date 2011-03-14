using System;
using System.Collections.Generic;

namespace NDistribUnit.Common.Logging
{
    /// <summary>
    /// Represents a log, which contains a fixed number of entries.
    /// After the number of those entries exceeds the maximum number of items, new items begin to overwrite old ones.
    /// </summary>
    public class RollingLog: ILog
    {
        private readonly int logEntriesCount;

        /// <summary>
        /// Initializes a new instance of <see cref="RollingLog"/>
        /// </summary>
        /// <param name="logEntriesCount"></param>
        public RollingLog(int logEntriesCount)
        {
            this.logEntriesCount = logEntriesCount;
        }

        /// <summary>
        /// Logs the beginning of some activity. It is supposed, that a corresponding EndActivity is always called.
        /// </summary>
        /// <param name="message">A message, which describes the activity</param>
        public void BeginActivity(string message)
        {
            AddItem(LogEntryType.ActivityStart, message);
        }

        private void AddItem(LogEntryType type, string message)
        {
            lock (this)
            {
                var list = new LinkedList<LogEntry>();
            }
        }

        /// <summary>
        /// Logs the ending of some activity. It is supposed, that a corresponding EndActivity is always called.
        /// </summary>
        /// <param name="message">A message, which describes the activity</param>
        public void EndActivity(string message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Logs some information
        /// </summary>
        /// <param name="message">A message to be logged</param>
        public void Info(string message)
        {
            throw new NotImplementedException();
        }

        ///
        public void Success(string message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Logs some warning information
        /// </summary>
        /// <param name="message">The warning message</param>
        public void Warning(string message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Logs some warning information with the exception, that caused the warning
        /// </summary>
        /// <param name="message">The warning message</param>
        /// <param name="exception">The exception, which caused the warning</param>
        public void Warning(string message, Exception exception)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Logs some error information
        /// </summary>
        /// <param name="message">The error message</param>
        public void Error(string message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Logs some error information with the exception, that caused the error
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="exception">The exception, which caused the error</param>
        public void Error(string message, Exception exception)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the saved entries
        /// </summary>
        /// <param name="startEntryId">The starting id of some entry. Use "0" if you want to start with the very beginning</param>
        /// <param name="maxReturnCount">The maximum number of entries, which should be returned</param>
        public LogEntry[] GetEntries(int startEntryId, int maxReturnCount)
        {
            throw new NotImplementedException();
        }
    }
}