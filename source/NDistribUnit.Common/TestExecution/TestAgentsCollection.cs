using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Extensions;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// A collection of agents
    /// </summary>
    public class TestAgentsCollection
    {
        private readonly List<AgentInformation> agents = new List<AgentInformation>();

        /// <summary>
        /// Occurs when agent state changed.
        /// </summary>
        public event EventHandler<EventArgs<AgentInformation>> AgentFreed;

        /// <summary>
        /// Occurs when agent was disconnected
        /// </summary>
        public event EventHandler<EventArgs<AgentInformation>> AgentDisconnected;

        /// <summary>
        /// The synchronization object, which is used for thread safe access to that collection
        /// </summary>
        public readonly object SyncObject = new object();

        /// <summary>
        /// Gets the count.
        /// </summary>
        public decimal Count
        {
            get
            {
                lock (SyncObject)
                {
                    return agents.Count;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has free or busy agents.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has free or busy agents; otherwise, <c>false</c>.
        /// </value>
        public bool AreConnectedAvailable
        {
            get 
            { 
                var agent = Get(a => a.State != AgentState.Disconnected && a.State != AgentState.Error);
                return agent != null;
            }
        }

        /// <summary>
        /// Gets the name of the by.
        /// </summary>
        /// <param name="agentName">Name of the agent.</param>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        public AgentInformation GetBy(string agentName, EndpointAddress address = null)
        {
            return agents.FirstOrDefault(a => a.Name.Equals(agentName)
                                              && (address == null || a.Address.Equals(address)));
        }

        /// <summary>
        /// Gets an agent by specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>The agent or <value>null</value> if nothing found</returns>
        public AgentInformation Get(Func<AgentInformation, bool> condition)
        {
            lock (SyncObject)
            {
                return agents.FirstOrDefault(condition);
            }
        }

        private void OnAgentStateChanged(object sender, EventArgs<AgentState> oldState)
        {
            var agent = (AgentInformation) sender;
            if (agent.State == AgentState.Ready && oldState.Data != AgentState.Ready)
                AgentFreed.SafeInvoke(this, agent);
            else if (agent.State == AgentState.Disconnected && oldState.Data != AgentState.Disconnected)
                AgentDisconnected.SafeInvoke(this, agent);
        }


        /// <summary>
        /// Moves to busy.
        /// </summary>
        /// <param name="agent">The agent.</param>
        public void MarkBusy(AgentInformation agent)
        {
            agent.State = AgentState.Busy;
        }

        /// <summary>
        /// Adds the specified endpoint.
        /// </summary>
        /// <param name="newAgent">The new agent.</param>
        public AgentInformation Add(AgentInformation newAgent)
        {
            lock (SyncObject)
            {
                var agent = GetBy(newAgent.Name);
                
                if (agent == null)
                {
                    agent = Get(a => a.Address.Equals(newAgent.Address));

                    if (agent != null)
                    {
                        // The name of an existent agent has changed
                        agent.Name = newAgent.Name;
                    }
                    else
                    {
                        // Agent was NOT found either by name or by address
                        agents.Add(agent = newAgent);
                        agent.StateChanged += OnAgentStateChanged;
                        
                    }
                }

                agent.LastStatusUpdate = newAgent.LastStatusUpdate;
                agent.Address = newAgent.Address;
                agent.Version = newAgent.Version;

                return agent;
            }
        }

        /// <summary>
        /// Toes the array.
        /// </summary>
        /// <returns></returns>
        public AgentInformation[] ToArray()
        {
            lock(SyncObject)
            {
                return agents.ToArray();
            }
        }

        /// <summary>
        /// Gets the free.
        /// </summary>
        public IList<AgentInformation> GetFree()
        {
            lock (SyncObject)
            {
                return agents.FindAll(a => a.State == AgentState.Ready);
            }
        }

        /// <summary>
        /// Marks the free.
        /// </summary>
        /// <param name="agent">The agent.</param>
        public void MarkAsReady(AgentInformation agent)
        {
            agent.State = AgentState.Ready;
        }
    }
}