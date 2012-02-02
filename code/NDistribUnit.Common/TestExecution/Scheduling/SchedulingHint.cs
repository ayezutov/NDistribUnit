namespace NDistribUnit.Common.TestExecution.Scheduling
{
    /// <summary>
    /// Hints for scheduler
    /// </summary>
    public enum SchedulingHint
    {
        /// <summary>
        /// Agent should be used in the first order 
        /// </summary>
        Preffer,

        /// <summary>
        /// Matching agent use should be avoided (but is still possible)
        /// </summary>
        NotRecommended,

        /// <summary>
        /// Only this agent should be used
        /// </summary>
        Force,

        /// <summary>
        /// Agent should not be used
        /// </summary>
        Block
    }
}