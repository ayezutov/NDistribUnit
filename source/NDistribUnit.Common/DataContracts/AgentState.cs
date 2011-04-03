namespace NDistribUnit.Common.DataContracts
{
    /// <summary>
    /// Describes the state of an agent
    /// </summary>
    public enum AgentState
    {
        /// <summary>
        /// The agent is connected and resides in idle state
        /// </summary>
        Connected,

        /// <summary>
        /// The agent is busy with some task
        /// </summary>
        Busy,

        /// <summary>
        /// The agent is not connecterd anymore
        /// </summary>
        Disconnected
    }
}