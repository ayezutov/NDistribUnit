using System;
using System.ServiceModel;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Common.Server.AgentsTracking
{
    /// <summary>
    /// 
    /// </summary>
    public class AgentUpdater : IAgentUpdater
    {
        private readonly IConnectionProvider connectionProvider;
        private readonly IVersionProvider versionProvider;
        private IUpdateSource updateSource;
        private AgentsCollection agents;
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentUpdater"/> class.
        /// </summary>
        /// <param name="connectionProvider">The connection provider.</param>
        /// <param name="versionProvider">The version provider.</param>
        /// <param name="updateSource">The update source.</param>
        /// <param name="agents">The agents.</param>
        /// <param name="log">The log.</param>
        public AgentUpdater(IConnectionProvider connectionProvider, IVersionProvider versionProvider, IUpdateSource updateSource, AgentsCollection agents, ILog log)
        {
            this.connectionProvider = connectionProvider;
            this.versionProvider = versionProvider;
            this.updateSource = updateSource;
            this.agents = agents;
            this.log = log;
        }

        /// <summary>
        /// Updates the agent.
        /// </summary>
        /// <param name="agent">The agent.</param>
        public void UpdateAgent(AgentMetadata agent)
        {
            try
            {
                var agentVersion = agent.Version;
                var currentVersion = versionProvider.GetVersion();

                if (agentVersion < currentVersion)
                {
                    var zippedVersionFolder = updateSource.GetZippedVersionFolder();
                    if (zippedVersionFolder != null)
                    {
                        agents.MarkAsUpdating(agent);
                        new Action(() =>
                                       {
                                           try
                                           {
                                               var testRunnerAgent =
                                                   connectionProvider.GetConnection<IRemoteAppPart>(agent.RemotePartAddress);

                                               testRunnerAgent.ReceiveUpdatePackage(new UpdatePackage
                                                                                        {
                                                                                            IsAvailable = true,
                                                                                            Version = currentVersion,
                                                                                            UpdateZipStream =
                                                                                                zippedVersionFolder
                                                                                        });
                                               agents.MarkAsUpdated(agent);
                                           }
                                           catch(CommunicationException ex)
                                           {
                                               agents.MarkAsDisconnected(agent);
                                               log.Error("Error while updating agent", ex);
                                               throw;
                                           }
                                           catch(Exception ex)
                                           {
                                               agents.MarkAsFailure(agent);
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
                agents.MarkAsFailure(agent);
            }
        }
    }
}