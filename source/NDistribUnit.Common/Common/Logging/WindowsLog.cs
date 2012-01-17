using System;
using System.Diagnostics;

namespace NDistribUnit.Common.Logging
{
    /// <summary>
    /// 
    /// </summary>
    public class WindowsLog : ILog
    {
        private string source;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsLog"/> class.
        /// </summary>
        /// <param name="postfix">The postfix.</param>
        public WindowsLog(string postfix)
        {
            source = string.Format("NDistribUnit {0}", postfix);
            if (!EventLog.SourceExists(source))
                EventLog.CreateEventSource(source, "NDistribUnit");
        }

        /// <summary>
        /// Logs the beginning of some activity. It is supposed, that a corresponding EndActivity is always called.
        /// </summary>
        /// <param name="message">A message, which describes the activity</param>
        public void BeginActivity(string message)
        {
        }

        /// <summary>
        /// Logs the ending of some activity. It is supposed, that a corresponding EndActivity is always called.
        /// </summary>
        /// <param name="message">A message, which describes the activity</param>
        public void EndActivity(string message)
        {
        }

        /// <summary>
        /// Logs some information
        /// </summary>
        /// <param name="message">A message to be logged</param>
        public void Info(string message)
        {
        }

        /// <summary>
        /// Logs successful event
        /// </summary>
        /// <param name="message">A message to be logged</param>
        public void Success(string message)
        {
        }

        /// <summary>
        /// Logs some warning information
        /// </summary>
        /// <param name="message">The warning message</param>
        public void Warning(string message)
        {
        }

        /// <summary>
        /// Logs some warning information with the exception, that caused the warning
        /// </summary>
        /// <param name="message">The warning message</param>
        /// <param name="exception">The exception, which caused the warning</param>
        public void Warning(string message, Exception exception)
        {
            new EventLog("NDistribUnit") { Source = source }.WriteEntry(message, EventLogEntryType.Warning);
        }

        /// <summary>
        /// Logs some error information
        /// </summary>
        /// <param name="message">The error message</param>
        public void Error(string message)
        {
            new EventLog("NDistribUnit"){Source = source}.WriteEntry(message, EventLogEntryType.Error);
        }

        /// <summary>
        /// Logs some error information with the exception, that caused the error
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="exception">The exception, which caused the error</param>
        public void Error(string message, Exception exception)
        {
            new EventLog("NDistribUnit"){Source = source}.WriteEntry(message + LoggingUtility.GetExceptionText(exception), EventLogEntryType.Error);
        }

        /// <summary>
        /// Logs some debugging information
        /// </summary>
        /// <param name="message">The message.</param>
        public void Debug(string message)
        {}
    }
}