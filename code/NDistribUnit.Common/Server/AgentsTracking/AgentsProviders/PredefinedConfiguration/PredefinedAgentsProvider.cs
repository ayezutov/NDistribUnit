using System;
using System.ServiceModel;
using System.Threading;
using NDistribUnit.Common.Extensions;

namespace NDistribUnit.Common.Server.AgentsTracking.AgentsProviders.PredefinedConfiguration
{
    /// <summary>
    /// 
    /// </summary>
    public class PredefinedAgentsProvider: IAgentsProvider
    {
        private readonly PredefinedAgentsConfigurationSection agents;
        private Timer timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PredefinedAgentsProvider"/> class.
        /// </summary>
        /// <param name="agents">The agents.</param>
        public PredefinedAgentsProvider(PredefinedAgentsConfigurationSection agents)
        {
            this.agents = agents;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            timer = new Timer(OnTimer, null, TimeSpan.FromSeconds(1), agents.RecheckInterval);
        }

        private void OnTimer(object state)
        {
            foreach (PredefinedAgentsElement agent in agents.Agents)
            {
                Connected.SafeInvoke(this, new EndpointAddress(agent.Url));
            }

        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            if (timer != null)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                timer = null;
            }
        }

        /// <summary>
        /// Occurs when an agent is connected.
        /// </summary>
        public event EventHandler<EventArgs<EndpointAddress>> Connected;
    }
}