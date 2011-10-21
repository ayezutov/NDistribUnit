namespace NDistribUnit.Common.Logging
{
    /// <summary>
    /// Describes a type of a log entry
    /// </summary>
    public enum LogEntryType
    {
        /// <summary>
        /// Start of some activity
        /// </summary>
        ActivityStart,

        /// <summary>
        /// End of some activity
        /// </summary>
        ActivityEnd,

        /// <summary>
        /// Information about some event
        /// </summary>
        Info,

        /// <summary>
        /// Warning notification
        /// </summary>
        Warning,

        /// <summary>
        /// Success notification
        /// </summary>
        Success,

        /// <summary>
        /// Error/Failure notification
        /// </summary>
        Error
    }
}