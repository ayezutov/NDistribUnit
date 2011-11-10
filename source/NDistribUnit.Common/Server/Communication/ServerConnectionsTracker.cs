using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using NDistribUnit.Common.Agent;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Communication.ConnectionTracking;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Common.Server.Communication
{
    /// <summary>
    /// Tracks connections on NDistribUnit server. It always has a list of agents with actual statuses.
    /// </summary>
    public class ServerConnectionsTracker
    {
        private readonly ILog log;
        private readonly INetworkExplorer<IRemoteParticle> networkExplorer;
        private readonly IUpdateSource updateSource;
        private readonly IVersionProvider versionProvider;
        private readonly IConnectionProvider connectionProvider;
        private readonly TestAgentsCollection agents;


        /// <summary>
        /// Initializes a new instance of a connection tracker
        /// </summary>
        /// <param name="networkExplorer">The connections tracker.</param>
        /// <param name="updateSource">The update source.</param>
        /// <param name="versionProvider">The version provider.</param>
        /// <param name="connectionProvider">The connection provider.</param>
        /// <param name="log">The log.</param>
        /// <param name="agents">The agents.</param>
        public ServerConnectionsTracker(
            INetworkExplorer<IRemoteParticle> networkExplorer,
            IUpdateSource updateSource,
            IVersionProvider versionProvider,
            IConnectionProvider connectionProvider,
            ILog log,
            TestAgentsCollection agents)
        {
            this.log = log;
            this.agents = agents;
            this.networkExplorer = networkExplorer;
            this.updateSource = updateSource;
            this.versionProvider = versionProvider;
            this.connectionProvider = connectionProvider;
            this.networkExplorer.EndpointConnected += OnEndpointConnected;
            this.networkExplorer.EndpointDisconnected += OnEndpointDisconnected;
            this.networkExplorer.EndpointSuccessfulPing += OnEndpointSuccessfulPing;
        }

        private void OnEndpointSuccessfulPing(object sender, EndpointConnectionChangedEventArgs e)
        {
            var savedAgent = agents.GetBy(e.EndpointInfo.Name);
            if (savedAgent != null)
            {
                if (e.Version > savedAgent.Version)
                {
                    savedAgent.Version = e.Version;
                    if (savedAgent.State == AgentState.Updating)
                        savedAgent.State = AgentState.Ready;
                }
                savedAgent.LastStatusUpdate = DateTime.UtcNow;
            }
        }

        private void OnEndpointConnected(object sender, EndpointConnectionChangedEventArgs e)
        {
            var agent = agents.Add(new AgentInformation
                                       {
                                           Address = e.EndpointInfo.Endpoint.Address,
                                           Name = e.EndpointInfo.Name,
                                           LastStatusUpdate = e.EndpointInfo.LastStatusUpdateTime,
                                           Version = e.EndpointInfo.Version
                                       });
            agent.State = AgentState.Ready;
            UpdateAgentIfRequired(agent);
        }

        private void UpdateAgentIfRequired(AgentInformation agent)
        {
            try
            {
                var agentVersion = agent.Version;
                var currentVersion = versionProvider.GetVersion();

                if (agentVersion < currentVersion)
                {
                    var zippedVersionFolder = updateSource.GetZippedVersionFolder(currentVersion);
                    if (zippedVersionFolder != null)
                    {
                        agent.State = AgentState.Updating;
                        new Action(() =>
                                       {
                                           try
                                           {
                                               var testRunnerAgent =
                                                   connectionProvider.GetConnection<IRemoteParticle>(
                                                       new EndpointAddress(new Uri(agent.Address.Uri,
                                                                                   AgentHost.RemoteParticleAddress)));
                                               testRunnerAgent.ReceiveUpdatePackage(new UpdatePackage
                                                                                        {
                                                                                            IsAvailable = true,
                                                                                            Version = currentVersion,
                                                                                            UpdateZipBytes =
                                                                                                zippedVersionFolder
                                                                                        });
                                           }
                                           catch(Exception ex)
                                           {
                                               log.Error("Error while updating agent", ex);
                                               throw;
                                           }
                                       }).BeginInvoke(null, null);
                    }
                }
            }
            catch (CommunicationException ex)
            {
                log.Error("Error while trying to check version or apply updates", ex);
                agent.State = AgentState.Error;
            }
        }


        private void OnEndpointDisconnected(object sender, EndpointConnectionChangedEventArgs e)
        {
            var savedAgent = agents.GetBy(e.EndpointInfo.Name);
            if (savedAgent != null)
            {
                savedAgent.LastStatusUpdate = DateTime.UtcNow;
                savedAgent.State = AgentState.Disconnected;
            }
        }


        /// <summary>
        /// Starts listening on agents' availability and monitors connected agents' statuses.
        /// </summary>
        public void Start()
        {
            networkExplorer.Start();
        }

        /// <summary>
        /// Stops listening for agents
        /// </summary>
        public void Stop()
        {
            networkExplorer.Stop();
        }
    }
}