using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Server.AgentsTracking.AgentsProviders;
using NDistribUnit.Common.Common.Extensions;

namespace NDistribUnit.Common.Server.AgentsTracking
{
    /// <summary>
    /// Pings free agents to be sure, that no have been disconnected
    /// </summary>
    public class AgentsTracker
    {
        private readonly AgentsCollection agents;
        private readonly IConnectionsHostOptions options;
        private readonly ILog log;
        private readonly IEnumerable<IAgentsProvider> providers;
        private readonly IConnectionProvider connectionProvider;
        private readonly IAgentUpdater agentUpdater;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentsTracker"/> class.
        /// </summary>
        /// <param name="agents">The agents.</param>
        /// <param name="options">The options.</param>
        /// <param name="log">The log.</param>
        /// <param name="providers">The providers.</param>
        /// <param name="connectionProvider">The connection provider.</param>
        /// <param name="agentUpdater">The agent updater.</param>
        public AgentsTracker(AgentsCollection agents, IConnectionsHostOptions options, ILog log,
                             IEnumerable<IAgentsProvider> providers, IConnectionProvider connectionProvider, IAgentUpdater agentUpdater)
        {
            this.agents = agents;
            this.options = options;
            this.log = log;
            this.providers = providers;
            this.connectionProvider = connectionProvider;
            this.agentUpdater = agentUpdater;
        }

        /// <summary>
        /// Starts the tracker.
        /// </summary>
        public void Start()
        {
            foreach (var provider in providers)
            {
                provider.Connected += OnAgentConnected;
                provider.Start();
            }
        }

        /// <summary>
        /// Starts the tracker.
        /// </summary>
        public void Stop()
        {
            foreach (var provider in providers)
            {
                provider.Stop();
                provider.Connected -= OnAgentConnected;
            }
        }
        
        private void OnAgentConnected(object sender, EventArgs<EndpointAddress> e)
        {
            var agent = new AgentMetadata(e.Data);

            using (agents.Lock())
            {
                var savedAgent = agents.GetAgentByAddress(agent.Address);
                if (savedAgent != null && !savedAgent.Status.IsOneOf(AgentState.Disconnected, AgentState.Updated))
                    return;
            }

            var action =
                new Action(
                    () =>
                        {
                            try
                            {
                                LockCallToAddress(agent.Address);

                                log.Debug(string.Format("Initially pinging {0}", agent.Address));
                                var remoteAppPart = connectionProvider.GetConnection<IRemoteAppPart>(agent.RemotePartAddress);
                                
                                var result = remoteAppPart.Ping(TimeSpan.FromMilliseconds(options.PingIntervalInMiliseconds));

                                log.Debug(string.Format("Successfully initially pinged {0} at {1}", result.Name, agent.Address));

                                agent.Name = result.Name;
                                agent.Version = result.Version;

                                AddToCollection(agent);
                            }
                            finally
                            {
                                UnlockCallToAddress(agent.Address);
                            }
                        });

            action.BeginInvoke(ar =>
                                   {
                                       try
                                       {
                                           action.EndInvoke(ar);
                                       }
                                       catch(Exception ex)
                                       {
                                           // this should be traced as error
                                           log.Debug(string.Format("An error occurred, while initially pinging and adding to collection the endpoint ({0}): {1}", agent.Address, ex.Message));
                                       }
                                   }, null);
        }

        private void AddToCollection(AgentMetadata agent)
        {
            using (agents.Lock())
            {
                // ReSharper disable AccessToModifiedClosure
                var agentWithSameNameSameAddress = agents.GetAgent(a => string.Equals(a.Name, agent.Name)
                                                                    && a.Address.Equals(agent.Address));
                var agentWithSameName = agents.GetAgentByName(agent.Name);
                var agentWithSameAddress = agents.GetAgentByAddress(agent.Address);
                // ReSharper restore AccessToModifiedClosure

                if (agentWithSameNameSameAddress != null && agentWithSameNameSameAddress.Status == AgentState.Updated)
                {
                    if (agentWithSameNameSameAddress.Version < agent.Version) // agent was updated and restarted

                        StopAgentPinging(agentWithSameNameSameAddress);
                    else
                        return; // State was set to updated, but the agent has not been restarted yet
                }
                else if (agentWithSameNameSameAddress != null && agentWithSameNameSameAddress.Status != AgentState.Disconnected)
                    return;

                if (agentWithSameNameSameAddress != null) //agent is disconnected here
                {
                    agentWithSameNameSameAddress.Version = agent.Version;
                    agent = agentWithSameNameSameAddress; // replace, so it can be later connected
                }
                else
                {
                    if (agentWithSameName != null)
                    {
                        StopAgentPinging(agentWithSameName);
                        agents.MarkAsDisconnected(agentWithSameName);
                    }

                    if (agentWithSameAddress != null)
                    {
                        StopAgentPinging(agentWithSameAddress);
                        agents.MarkAsDisconnected(agentWithSameAddress);
                    }
                }
                
                agents.Connect(agent);
                StartAgentPinging(agent);
            }
        }


        readonly ConcurrentDictionary<AgentMetadata, Timer> timers = new ConcurrentDictionary<AgentMetadata, Timer>();
        private void StopAgentPinging(AgentMetadata agent)
        {
            Timer timer;
            if (timers.TryRemove(agent, out timer))
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                timer.Dispose();
            }
        }

        private void StartAgentPinging(AgentMetadata agent)
        {
            var timer = timers.GetOrAdd(agent, key => new Timer(OnPingTimerTick, key, Timeout.Infinite, Timeout.Infinite));
            lock (timer)
            {
                timer.Change(options.PingIntervalInMiliseconds, Timeout.Infinite);
            }
        }

        private void OnPingTimerTick(object state)
        {
            // This method is run in a separate thread.
            // There is no need to try to make it asynchronous.

            var agent = state as AgentMetadata;

            Timer timer;
            if (!timers.TryGetValue(agent, out timer))
                return;

            if (agent == null || (!agent.Status.IsOneOf(new[] { AgentState.Ready, AgentState.New, AgentState.Error })))
            {
                ReschedulePinging(timer);
                return;
            }

            try
            {
                var agentRemote = connectionProvider.GetConnection<IRemoteAppPart>(agent.RemotePartAddress);
                var result = agentRemote.Ping(TimeSpan.FromMilliseconds(options.PingIntervalInMiliseconds));

                agent.Version = result.Version;

                if (!string.Equals(agent.Name, result.Name))
                {
                    agents.MarkAsDisconnected(agent);
                    var newAgent = new AgentMetadata(agent.Address)
                                       {
                                           Name = result.Name,
                                           Version = result.Version
                                       };
                    agents.Connect(newAgent);
                    StartAgentPinging(newAgent);
                }

                agentUpdater.UpdateAgent(agent);

                ReschedulePinging(timer);
            }
            catch (CommunicationException)
            {
                // remove agent from tracking
                // mark as disconnected
                StopAgentPinging(agent);
                agents.MarkAsDisconnected(agent);
            }
            catch // in case of any other exception, we assume, that the agent errored out.
            {
                // remove agent from tracking
                // mark as errored out
                StopAgentPinging(agent);
                agents.MarkAsFailure(agent);
            }
        }

        private void ReschedulePinging(Timer timer)
        {
            try
            {
                if (timer != null)
                    timer.Change(options.PingIntervalInMiliseconds,
                                 Timeout.Infinite);
            }
            catch (ObjectDisposedException)
            {
                // Do not handle this: timer was destroyed by another thread
            }
        }

        private readonly ConcurrentDictionary<string, object> addressLockObjects = new ConcurrentDictionary<string, object>();

        private void UnlockCallToAddress(EndpointAddress address)
        {
            object lockObject;
            if (addressLockObjects.TryGetValue(address.ToString(), out lockObject))
            {
                Monitor.Exit(lockObject);
            }
        }

        private void LockCallToAddress(EndpointAddress address)
        {
            var valueToLock = addressLockObjects.GetOrAdd(address.ToString(), k => new object());
            Monitor.Enter(valueToLock);
        }
    }
}