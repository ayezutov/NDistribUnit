using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Communication.ConnectionTracking;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Extensions;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Common.Server.Communication
{
    /// <summary>
    /// Tracks connections on NDistribUnit server. It always has a list of agents with actual statuses.
    /// </summary>
    public class ServerConnectionsTracker
    {
        private readonly ILog log;
        private readonly INetworkExplorer<ITestRunnerAgent> networkExplorer;
    	private readonly IUpdateSource updateSource;
        private readonly IVersionProvider versionProvider;
        private readonly IConnectionProvider connectionProvider;

        /// <summary>
        /// Gets the list of agents
        /// </summary>
        public IList<AgentInformation> Agents { get; private set; }

        /// <summary>
		/// Occurs when agent state changed.
		/// </summary>
    	public event EventHandler<EventArgs<AgentInformation>> AgentFreed;

        /// <summary>
        /// Occurs when agent was disconnected
        /// </summary>
        public event EventHandler<EventArgs<AgentInformation>> AgentDisconnected;

        /// <summary>
        /// Initializes a new instance of a connection tracker
        /// </summary>
        /// <param name="networkExplorer">The connections tracker.</param>
        /// <param name="updateSource">The update source.</param>
        /// <param name="versionProvider">The version provider.</param>
        /// <param name="connectionProvider">The connection provider.</param>
        /// <param name="log">The log.</param>
        public ServerConnectionsTracker(
            INetworkExplorer<ITestRunnerAgent> networkExplorer, 
            IUpdateSource updateSource, 
            IVersionProvider versionProvider, 
            IConnectionProvider connectionProvider,
            ILog log)
        {
            this.log = log;
            Agents = new List<AgentInformation>();
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
            lock (Agents)
            {
                 var savedAgent = FindAgentByName(e.EndpointInfo.Name);
                 if (savedAgent != null)
                 {
					 if (e.Version > savedAgent.Version)
					 {
					 	savedAgent.Version = e.Version;
						 if (savedAgent.State == AgentState.Updating)
							 savedAgent.State = AgentState.Connected;
					 }
                 	savedAgent.LastStatusUpdate = DateTime.UtcNow;
                 }
            }
        }

        void OnEndpointConnected(object sender, EndpointConnectionChangedEventArgs e)
        {
            lock (Agents)
            {
            	var agent = AddAgentToList(e);
            	UpdateAgentIfRequired(agent);
            }
        	return;
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
                                        var testRunnerAgent = connectionProvider.GetConnection<ITestRunnerAgent>(agent.Endpoint.Address);
    					           		testRunnerAgent.ReceiveUpdatePackage(new UpdatePackage
    					           		                                     	{
    					           		                                     		IsAvailable = true,
																					Version = currentVersion,
																					UpdateZipBytes = zippedVersionFolder
    					           		                                     	});
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

    	private AgentInformation AddAgentToList(EndpointConnectionChangedEventArgs e)
    	{
    		var savedAgent = FindAgentByName(e.EndpointInfo.Name);
    		if (savedAgent == null)
    		{
    			savedAgent = Agents.FirstOrDefault(a => a.Endpoint.Address.Equals(e.EndpointInfo.Endpoint.Address));

    			if (savedAgent != null)
    			{
    				savedAgent.LastStatusUpdate = DateTime.UtcNow;
    				savedAgent.Name = e.EndpointInfo.Name;
    			    savedAgent.Version = e.EndpointInfo.Version;
    			    savedAgent.State = AgentState.Connected;
    			}
    			else
    			{
    				savedAgent = new AgentInformation
    				             	{
    				             		Endpoint = e.EndpointInfo.Endpoint,
    				             		LastStatusUpdate = DateTime.UtcNow,
    				             		Name = e.EndpointInfo.Name,
    				             		Version = e.EndpointInfo.Version
    				             	};
    				savedAgent.StateChanged += OnAgentStateChanged;
    				savedAgent.State = AgentState.Connected;
    				Agents.Add(savedAgent);
    			}
    		}
    		else
    		{
    			if (savedAgent.State == AgentState.Disconnected)
    			{
    				savedAgent.State = AgentState.Connected;
    			}
    			savedAgent.LastStatusUpdate = DateTime.UtcNow;
    			savedAgent.Endpoint = e.EndpointInfo.Endpoint;
    			savedAgent.Version = e.EndpointInfo.Version;
    		}
    		return savedAgent;
    	}

    	private void OnAgentStateChanged(object sender, EventArgs<AgentState> oldState)
    	{
    	    var agent = (AgentInformation)sender;
			if (agent.State == AgentState.Connected && oldState.Data != AgentState.Connected)
    			AgentFreed.SafeInvoke(this, agent);
            else if (agent.State == AgentState.Disconnected && oldState.Data != AgentState.Disconnected)
    			AgentDisconnected.SafeInvoke(this, agent);
    	}

    	private void OnEndpointDisconnected(object sender, EndpointConnectionChangedEventArgs e)
        {
            lock (Agents)
            {
                var savedAgent = FindAgentByName(e.EndpointInfo.Name);
                if (savedAgent != null)
                {
                    savedAgent.LastStatusUpdate = DateTime.UtcNow;
                    savedAgent.State = AgentState.Disconnected;
                }
            }
        }

        private AgentInformation FindAgentByName(string name)
        {
            return Agents.FirstOrDefault(agent => agent.Name.Equals(name));
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

        /// <summary>
        /// Moves to busy.
        /// </summary>
        /// <param name="agent">The agent.</param>
        public void MarkBusy(AgentInformation agent)
        {
            agent.State = AgentState.Busy;
        }
    }
}