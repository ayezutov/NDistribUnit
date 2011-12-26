
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
        New,

        /// <summary>
        /// The agent is connected and resides in idle state
        /// </summary>
        Ready,

        /// <summary>
        /// The agent is busy with some task
        /// </summary>
        Busy,

        /// <summary>
        /// The agent is not connecterd anymore
        /// </summary>
        Disconnected,

		/// <summary>
		/// An exception was thrown, when trying to communicate with that agent last time
		/// </summary>
    	Error,

		/// <summary>
		/// The agent is beeing updated
		/// </summary>
    	Updating,

        /// <summary>
        /// The agent was updated and should be being restarted
        /// </summary>
        Updated
    }
}