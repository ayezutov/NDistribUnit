using System;

namespace NDistribUnit.Common.Logging
{
    /// <summary>
    /// A log, which is able to write to multiple child logs
    /// </summary>
    public class CombinedLog: ILog
    {
        private readonly ILog[] logs;

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="logs">Child logs, which should be written to.</param>
        public CombinedLog(params ILog[] logs)
        {
            this.logs = logs;
        }

        /// <summary>
        /// Logs the beginning of some activity. It is supposed, that a corresponding EndActivity is always called.
        /// </summary>
        /// <param name="message">A message, which describes the activity</param>
        public void BeginActivity(string message)
        {
            foreach (var logger in logs)
            {
                logger.BeginActivity(message);
            }
        }

        /// <summary>
        /// Logs the ending of some activity. It is supposed, that a corresponding EndActivity is always called.
        /// </summary>
        /// <param name="message">A message, which describes the activity</param>
        public void EndActivity(string message)
        {
            foreach (var logger in logs)
            {
                logger.EndActivity(message);
            }
        }

        /// <summary>
        /// Logs some information
        /// </summary>
        /// <param name="message">A message to be logged</param>
        public void Info(string message)
        {
            foreach (var logger in logs)
            {
                logger.Info(message);
            }
        }

        ///
        public void Success(string message)
        {
            foreach (var logger in logs)
            {
                logger.Success(message);
            }
        }

        /// <summary>
        /// Logs some warning information
        /// </summary>
        /// <param name="message">The warning message</param>
        public void Warning(string message)
        {
            foreach (var logger in logs)
            {
                logger.Warning(message);
            }
        }

        /// <summary>
        /// Logs some warning information with the exception, that caused the warning
        /// </summary>
        /// <param name="message">The warning message</param>
        /// <param name="exception">The exception, which caused the warning</param>
        public void Warning(string message, Exception exception)
        {
            foreach (var logger in logs)
            {
                logger.Warning(message, exception);
            }
        }

        /// <summary>
        /// Logs some error information
        /// </summary>
        /// <param name="message">The error message</param>
        public void Error(string message)
        {
            foreach (var logger in logs)
            {
                logger.Error(message);
            }
        }

        /// <summary>
        /// Logs some error information with the exception, that caused the error
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="exception">The exception, which caused the error</param>
        public void Error(string message, Exception exception)
        {
            foreach (var logger in logs)
            {
                logger.Error(message, exception);
            }
        }

		/// <summary>
		/// Logs some debugging information
		/// </summary>
		/// <param name="message">The message.</param>
    	public void Debug(string message)
    	{
#if DEBUG
			foreach (var logger in logs)
			{
				logger.Debug(message);
			}
#endif
    	}
    }
}
