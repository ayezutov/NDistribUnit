using System;

namespace NDistribUnit.Common.Logging
{
    /// <summary>
    /// Represents a class, which is able to perform logging 
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// Logs the beginning of some activity. It is supposed, that a corresponding EndActivity is always called.
        /// </summary>
        /// <param name="message">A message, which describes the activity</param>
        void BeginActivity(string message);

        /// <summary>
        /// Logs the ending of some activity. It is supposed, that a corresponding EndActivity is always called.
        /// </summary>
        /// <param name="message">A message, which describes the activity</param>
        void EndActivity(string message);

        /// <summary>
        /// Logs some information
        /// </summary>
        /// <param name="message">A message to be logged</param>
        void Info(string message);

        /// <summary>
        /// Logs successful event
        /// </summary>
        /// <param name="message">A message to be logged</param>
        void Success(string message);

        /// <summary>
        /// Logs some warning information
        /// </summary>
        /// <param name="message">The warning message</param>
        void Warning(string message);

        /// <summary>
        /// Logs some warning information with the exception, that caused the warning
        /// </summary>
        /// <param name="message">The warning message</param>
        /// <param name="exception">The exception, which caused the warning</param>
        void Warning(string message, Exception exception);

        /// <summary>
        /// Logs some error information
        /// </summary>
        /// <param name="message">The error message</param>
        void Error(string message);

        /// <summary>
        /// Logs some error information with the exception, that caused the error
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="exception">The exception, which caused the error</param>
        void Error(string message, Exception exception);

		/// <summary>
		/// Logs some debugging information
		/// </summary>
		/// <param name="message">The message.</param>
		void Debug(string message);
    }
}