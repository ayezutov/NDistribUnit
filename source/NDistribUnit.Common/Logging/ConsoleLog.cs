using System;
using System.IO;

namespace NDistribUnit.Common.Logging
{
    /// <summary>
    /// Represents a log, which displays the output in console.
    /// </summary>
    public class ConsoleLog: ILog
    {
        /// <summary>
        /// Logs the beginning of some activity. It is supposed, that a corresponding EndActivity is always called.
        /// </summary>
        /// <param name="message">A message, which describes the activity</param>
        public void BeginActivity(string message)
        {
            WriteToConsole(System.Console.Out, ConsoleColor.White, "Start:", message, null);
        }

        /// <summary>
        /// Logs the ending of some activity. It is supposed, that a corresponding EndActivity is always called.
        /// </summary>
        /// <param name="message">A message, which describes the activity</param>
        public void EndActivity(string message)
        {
            WriteToConsole(System.Console.Out, ConsoleColor.DarkGray, "End  :", message, null);
        }

        /// <summary>
        /// Logs some information
        /// </summary>
        /// <param name="message">A message to be logged</param>
        public void Info(string message)
        {
            WriteToConsole(System.Console.Out, ConsoleColor.Gray, "Info :", message, null);
        }

        /// <summary>
        /// Logs some successful message
        /// </summary>
        /// <param name="message"></param>
        public void Success(string message)
        {
            WriteToConsole(System.Console.Out, ConsoleColor.Green, "Ok   :", message, null);
        }

        /// <summary>
        /// Logs some warning information
        /// </summary>
        /// <param name="message">The warning message</param>
        public void Warning(string message)
        {
            WriteToConsole(System.Console.Out, ConsoleColor.Yellow, "Warn :", message, null);
        }

        /// <summary>
        /// Logs some warning information with the exception, that caused the warning
        /// </summary>
        /// <param name="message">The warning message</param>
        /// <param name="exception">The exception, which caused the warning</param>
        public void Warning(string message, Exception exception)
        {
            WriteToConsole(System.Console.Out, ConsoleColor.Yellow, "Warn :", message, exception);
        }

        /// <summary>
        /// Logs some error information
        /// </summary>
        /// <param name="message">The error message</param>
        public void Error(string message)
        {
            WriteToConsole(System.Console.Error, ConsoleColor.Red, "Error:", message, null);
        }

        /// <summary>
        /// Logs some error information with the exception, that caused the error
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="exception">The exception, which caused the error</param>
        public void Error(string message, Exception exception)
        {
            WriteToConsole(System.Console.Error, ConsoleColor.Red, "Error:", message, exception);
        }

        private static void WriteToConsole(TextWriter writer, ConsoleColor color, string prefix, string message, Exception exception)
        {
            if (exception != null)
            {
                message += LoggingUtility.GetExceptionText(exception);
            }
            lock(typeof(System.Console))
            {
                var oldColor = System.Console.ForegroundColor;
                System.Console.ForegroundColor = color;
                writer.WriteLine(prefix + message);
                System.Console.ForegroundColor = oldColor;
            }
        }
    }
}