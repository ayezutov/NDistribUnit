namespace NDistribUnit.Common.TestExecution.Data
{
    /// <summary>
    /// 
    /// </summary>
    public enum TestRunRequestStatus
    {
        /// <summary>
        /// The request was only created and was not added to processing yet
        /// </summary>
        None,
        
        /// <summary>
        /// The request was received and added to the queue. The corresponding tests were not yet extracted.
        /// </summary>
        Received,

        /// <summary>
        /// The request has been parsed and a list of tests in it was created. Tests execution has not started yet.
        /// </summary>
        Waiting,

        /// <summary>
        /// Tests execution has already been started (at least a single test was sent to execution)
        /// </summary>
        Pending,

        /// <summary>
        /// All tests have been executed. The results are available.
        /// </summary>
        Completed
    }
}