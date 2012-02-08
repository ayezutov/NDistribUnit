using System;
using System.Runtime.Serialization;

namespace NDistribUnit.Common.Common.Logging
{
    /// <summary>
    /// Serializable exception
    /// </summary>
    [DataContract]
    public class ExceptionEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionEntry"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public ExceptionEntry(Exception exception)
        {
            Message = exception.Message;
            StackTrace = exception.StackTrace;
            InnerException = exception.InnerException != null ? new ExceptionEntry(exception.InnerException) : null;
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the stack trace.
        /// </summary>
        /// <value>
        /// The stack trace.
        /// </value>
        [DataMember]
        public string StackTrace { get; set; }

        /// <summary>
        /// Gets or sets the inner exception.
        /// </summary>
        /// <value>
        /// The inner exception.
        /// </value>
        [DataMember]
        public ExceptionEntry InnerException { get; set; }
    }
}