using System;
using System.Linq;
using NDistribUnit.Common.Collections;
using NDistribUnit.Common.Logging;

namespace NDistribUnit.Common.Common.Logging
{
    /// <summary>
    /// Represents a log, which contains a fixed number of entries.
    /// After the number of those entries exceeds the maximum number of items, new items begin to overwrite old ones.
    /// </summary>
    public class RollingLog : ILog
    {
        private readonly RollingList<LogEntry> list;
        private int idCounter = 1;

        /// <summary>
        /// Initializes a new instance of <see cref="RollingLog"/>
        /// </summary>
        /// <param name="logEntriesCount"></param>
        public RollingLog(int logEntriesCount)
        {
            list = new RollingList<LogEntry>(logEntriesCount);
        }

        /// <summary>
        /// Logs the beginning of some activity. It is supposed, that a corresponding EndActivity is always called.
        /// </summary>
        /// <param name="message">A message, which describes the activity</param>
        public void BeginActivity(string message)
        {
            AddItem(LogEntryType.ActivityStart, message);
        }

        private void AddItem(LogEntryType type, string message, Exception exception = null)
        {
            lock (this)
            {
                list.Add(new LogEntry(GetNextId(), type, message, DateTime.UtcNow, exception));
            }
        }

        private int GetNextId()
        {
            return idCounter++;
        }

        /// <summary>
        /// Logs the ending of some activity. It is supposed, that a corresponding EndActivity is always called.
        /// </summary>
        /// <param name="message">A message, which describes the activity</param>
        public void EndActivity(string message)
        {
            AddItem(LogEntryType.ActivityEnd, message);
        }

        /// <summary>
        /// Logs some information
        /// </summary>
        /// <param name="message">A message to be logged</param>
        public void Info(string message)
        {
            AddItem(LogEntryType.Info, message);
        }

        /// <summary>
        /// Logs successful event
        /// </summary>
        /// <param name="message">A message to be logged</param>
        public void Success(string message)
        {
            AddItem(LogEntryType.Success, message);
        }

        /// <summary>
        /// Logs some warning information
        /// </summary>
        /// <param name="message">The warning message</param>
        public void Warning(string message)
        {
            AddItem(LogEntryType.Warning, message);
        }

        /// <summary>
        /// Logs some warning information with the exception, that caused the warning
        /// </summary>
        /// <param name="message">The warning message</param>
        /// <param name="exception">The exception, which caused the warning</param>
        public void Warning(string message, Exception exception)
        {
            AddItem(LogEntryType.Warning, message, exception);
        }

        /// <summary>
        /// Logs some error information
        /// </summary>
        /// <param name="message">The error message</param>
        public void Error(string message)
        {
            AddItem(LogEntryType.Error, message);
        }

        /// <summary>
        /// Logs some error information with the exception, that caused the error
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="exception">The exception, which caused the error</param>
        public void Error(string message, Exception exception)
        {
            AddItem(LogEntryType.Error, message, exception);
        }

		/// <summary>
		/// Logs some debugging information
		/// </summary>
		/// <param name="message">The message.</param>
    	public void Debug(string message)
    	{}

    	/// <summary>
        /// Gets the saved entries
        /// </summary>
        /// <param name="lastFetchedEntryId">The starting id of some entry. Use "0" if you want to start with the very beginning</param>
        /// <param name="maxReturnCount">The maximum number of entries, which should be returned</param>
        public LogEntry[] GetEntries(int? lastFetchedEntryId, int maxReturnCount)
        {
            // TODO: use hash-based collection (dictionary) for quick find

            RollingListItem<LogEntry> item = lastFetchedEntryId.HasValue 
                                       ? list.FindFirst(v => v.Id.Equals(lastFetchedEntryId.Value)) 
                                       : null;

            return item == null 
                ? list.GetHead(maxReturnCount).ToArray() 
                : list.GetItemsAfter(item, maxReturnCount).ToArray();
        }
    }
}