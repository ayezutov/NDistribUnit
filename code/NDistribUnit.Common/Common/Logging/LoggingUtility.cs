using System;
using System.Diagnostics;
using System.Text;
using System.ServiceModel;

namespace NDistribUnit.Common.Logging
{
    /// <summary>
    /// Common routines, which are used for logging
    /// </summary>
    public class LoggingUtility
    {
        /// <summary>
        /// Converts exception to a readable format
        /// </summary>
        /// <param name="exception">The exception, which should be converted</param>
        /// <returns>A readable representation of the exception</returns>
        public static string GetExceptionText(Exception exception)
        {
            if (exception == null)
                return String.Empty;

            var sb = new StringBuilder();

            GetExceptionText(exception, sb);

            return sb.ToString();
        }

        private static void GetExceptionText(Exception exception, StringBuilder stringBuilder)
        {
            Debug.Assert(exception != null);
            Debug.Assert(stringBuilder != null);

            stringBuilder.AppendLine();
            stringBuilder.AppendLine(exception.GetType().FullName);
            stringBuilder.AppendLine(exception.Message);
            stringBuilder.AppendLine(exception.StackTrace);

            var faultException = exception as FaultException<ExceptionDetail>;
            if (faultException != null)
            {
                var detail = faultException.Detail;
                GetExceptionText(detail, stringBuilder);
            }

            if (exception.InnerException != null)
            {
                stringBuilder.AppendLine("------------");
                GetExceptionText(exception.InnerException, stringBuilder);
            }
        }

        private static void GetExceptionText(ExceptionDetail exception, StringBuilder stringBuilder)
        {
            Debug.Assert(exception != null);
            Debug.Assert(stringBuilder != null);

            stringBuilder.AppendLine();
            stringBuilder.AppendLine(exception.Type);
            stringBuilder.AppendLine(exception.Message);
            stringBuilder.AppendLine(exception.StackTrace);

            if (exception.InnerException != null)
            {
                stringBuilder.AppendLine("------------");
                GetExceptionText(exception.InnerException, stringBuilder);
            }
        }
    }
}