using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using NDistribUnit.Common.Common;
using System.Linq;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Common.Extensions;
using NDistribUnit.Common.Extensions;
using NDistribUnit.Common.Logging;

namespace NDistribUnit.Common.Server.AgentsTracking
{
    /// <summary>
    /// The collection of agents
    /// </summary>
    public class AgentsCollection
    {
        private readonly ILog log;
        private readonly object syncObject = new object();
        private readonly List<AgentMetadata> allAgents = new List<AgentMetadata>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentsCollection"/> class.
        /// </summary>
        /// <param name="log">The log.</param>
        public AgentsCollection(ILog log)
        {
            this.log = log;
            allAgents = new List<AgentMetadata>();
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count
        {
            get { return allAgents.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether there are connected agents available.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if there are connected agents available; otherwise, <c>false</c>.
        /// </value>
        public bool AreConnectedAvailable
        {
            get {
                lock (syncObject)
                {
                    return allAgents.Any(a => !a.Status.IsOneOf(new[] { AgentState.Disconnected, AgentState.Error }));
                }
            }
        }

        /// <summary>
        /// Occurs when a ready agent appears (either when added to collection or when returning to ready state).
        /// </summary>
        public event EventHandler<EventArgs> ReadyAgentAppeared;

        /// <summary>
        /// Occurs when an agent gets disconnected or fails.
        /// </summary>
        public event EventHandler<EventArgs> ClientDisconnectedOrFailed;

        /// <summary>
        /// Locks this instance.
        /// </summary>
        /// <returns></returns>
        public IDisposable Lock()
        {
            Monitor.Enter(syncObject);
            return new Unlocker(()=>Monitor.Exit(syncObject));
        }

        /// <summary>
        /// Gets the agent.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns></returns>
        public AgentMetadata GetAgent(Func<AgentMetadata, bool> condition)
        {
            lock (syncObject)
            {
                return allAgents.FirstOrDefault(condition); 
            }
        }

        /// <summary>
        /// Gets the agent by address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        public AgentMetadata GetAgentByAddress(EndpointAddress address)
        {
            return GetAgent(a => a.Address.Equals(address));
        }
        /// <summary>
        /// Gets the agent by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public AgentMetadata GetAgentByName(string name)
        {
            return GetAgent(a => string.Equals(a.Name, name));
        }

        /// <summary>
        /// Connects the specified agent metadata.
        /// </summary>
        /// <param name="agent">The agent metadata.</param>
        public void Connect(AgentMetadata agent)
        {
            lock (syncObject)
            {
                ChangeStatus(agent, AgentState.Ready);
                allAgents.Add(agent);
            }
            
        }

        /// <summary>
        /// Gets a free agent.
        /// </summary>
        /// <returns></returns>
        public IList<AgentMetadata> GetFree()
        {
            lock (syncObject)
            {
                return allAgents.FindAll(a => a.Status == AgentState.Ready);
            }
        }

        private void ChangeStatus(AgentMetadata agent, AgentState agentState)
        {
            if (agent == null)
                return; 

            var oldStatus = agent.Status;
            ((IAgentStateSetter)agent).Status = agentState;

            if (oldStatus != agentState)
            {
                log.Debug(string.Format("Agent '{0}' state was changed from {1} to {2}", agent.Address, oldStatus, agentState));
            }

            if (oldStatus != AgentState.Ready && agentState == AgentState.Ready)
                ReadyAgentAppeared.SafeInvoke(this, EventArgs.Empty);
            else if (!oldStatus.IsOneOf(new[]{AgentState.Disconnected, AgentState.Error})
                && agentState.IsOneOf(new[]{AgentState.Disconnected, AgentState.Error}))
            {
                ClientDisconnectedOrFailed.SafeInvoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Disconnects the specified agent.
        /// </summary>
        /// <param name="agent">The agent.</param>
        public void MarkAsDisconnected(AgentMetadata agent)
        {
            lock (syncObject)
            {
                ChangeStatus(agent, AgentState.Disconnected);
            }
        }

        /// <summary>
        /// Marks as failure.
        /// </summary>
        /// <param name="agent">The agent.</param>
        public void MarkAsFailure(AgentMetadata agent)
        {
            lock (syncObject)
            {
                ChangeStatus(agent, AgentState.Error);
            }
        }

        /// <summary>
        /// Marks as busy.
        /// </summary>
        /// <param name="agent">The agent.</param>
        public void MarkAsBusy(AgentMetadata agent)
        {
            lock (syncObject)
            {
                ChangeStatus(agent, AgentState.Busy);
            }
        }

        /// <summary>
        /// Marks as ready.
        /// </summary>
        /// <param name="agent">The agent.</param>
        public void MarkAsReady(AgentMetadata agent)
        {
            lock (syncObject)
            {
                ChangeStatus(agent, AgentState.Ready);
            }
        }

        /// <summary>
        /// Marks the agent as updating.
        /// </summary>
        /// <param name="agent">The agent.</param>
        public void MarkAsUpdating(AgentMetadata agent)
        {
            lock (syncObject)
            {
                ChangeStatus(agent, AgentState.Updating);
            }
        }

        /// <summary>
        /// Marks as updated.
        /// </summary>
        /// <param name="agent">The agent.</param>
        public void MarkAsUpdated(AgentMetadata agent)
        {
            ChangeStatus(agent, AgentState.Updated);
        }

        /// <summary>
        /// Toes the array.
        /// </summary>
        /// <returns></returns>
        public AgentMetadata[] ToArray()
        {
            return allAgents.ToArray();
        }
    }
}