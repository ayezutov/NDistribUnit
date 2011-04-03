namespace NDistribUnit.Common.Agent
{
    /// <summary>
    /// An external module, which adds additional behavior to agent's service
    /// </summary>
    public interface IAgentExternalModule
    {
        /// <summary>
        /// Starts the module for the given host.
        /// </summary>
        /// <param name="host">The host.</param>
        void Start(AgentHost host);

        /// <summary>
        /// Stops the module for the given host.
        /// </summary>
        /// <param name="host">The host.</param>
        void Stop(AgentHost host);

        /// <summary>
        /// Aborts the module for the given host.
        /// </summary>
        /// <param name="host">The host.</param>
        void Abort(AgentHost host);
    }
}