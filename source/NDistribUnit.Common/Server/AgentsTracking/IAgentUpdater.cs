namespace NDistribUnit.Common.Server.AgentsTracking
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAgentUpdater
    {
        /// <summary>
        /// Updates the agent.
        /// </summary>
        /// <param name="agent">The agent.</param>
        void UpdateAgent(AgentMetadata agent);
    }
}