using System;
using System.ServiceModel;

namespace NDistribUnit.Common.Server.AgentsTracking.AgentsProviders
{
    /// <summary>
    /// A base class for providers of available agents
    /// </summary>
    public interface IAgentsProvider
    {
        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();

        /// <summary>
        /// Occurs when an agent is connected.
        /// </summary>
        event EventHandler<EventArgs<EndpointAddress>> Connected;
    }
}