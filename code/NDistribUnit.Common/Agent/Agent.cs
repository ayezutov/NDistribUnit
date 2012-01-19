using System;
using System.ServiceModel;
using NDistribUnit.Common.Common.Logging;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.TestExecution.DistributedConfiguration;
using NDistribUnit.Common.TestExecution.Storage;
using NUnit.Core;
using NDistribUnit.Common.Extensions;

namespace NDistribUnit.Common.Agent
{
    /// <summary>
    /// The service, which is communicated, when the server calls he agent
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class Agent : IAgent, IRemoteAppPart
    {
        private readonly ILog log;
    	private readonly RollingLog logStorage;
		private readonly IUpdateReceiver updateReceiver;
        private readonly IVersionProvider versionProvider;
        private readonly AgentTestRunner runner;
        private readonly IProjectsStorage projects;

        /// <summary>
        /// Initializes a new instance of the <see cref="Agent"/> class.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="logStorage">The log storage.</param>
        /// <param name="updateReceiver">The update receiver.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="versionProvider">The version provider.</param>
        /// <param name="runner">The runner.</param>
        /// <param name="projects">The projects.</param>
        public Agent(
            ILog log, 
            RollingLog logStorage, 
            IUpdateReceiver updateReceiver, 
            AgentConfiguration configuration, 
            IVersionProvider versionProvider, 
            AgentTestRunner runner, 
            IProjectsStorage projects)
        {
            this.log = log;
			this.logStorage = logStorage;
    		this.updateReceiver = updateReceiver;
            this.versionProvider = versionProvider;
            this.runner = runner;
            this.projects = projects;
            Name = configuration.AgentName;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public Version Version
        {
            get { return versionProvider.GetVersion(); }
        }
        
        /// <summary>
        /// Runs tests on agent
        /// </summary>
        /// <param name="test"></param>
        /// <param name="configurationSubstitutions"></param>
        /// <returns></returns>
        public TestResult RunTests(TestUnit test, DistributedConfigurationSubstitutions configurationSubstitutions)
        {
            log.Info(string.Format("Run Tests command Received: {0}", test.UniqueTestId));

            var result = runner.Run(test, configurationSubstitutions);
            result.ForSelfAndAllDescedants(r => r.SetAgentName(Name));

            return result;
        }

        /// <summary>
        /// Receives the project.
        /// </summary>
        /// <param name="project">The project.</param>
        public void ReceiveProject(ProjectMessage project)
        {
            projects.Store(project.TestRun, project.Project);
        }

        /// <summary>
        /// Receives the project.
        /// </summary>
        /// <param name="run">The run.</param>
        /// <returns>
        ///   <c>true</c> if the specified agent has a project for the given run; otherwise, <c>false</c>.
        /// </returns>
        public bool HasProject(TestRun run)
        {
            return projects.HasProject(run);
        }

        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <param name="maxItemsCount">The max items count.</param>
        /// <param name="lastFetchedEntryId">The last fetched entry id.</param>
        /// <returns></returns>
        public LogEntry[] GetLog(int maxItemsCount, int? lastFetchedEntryId)
        {
            return logStorage.GetEntries(lastFetchedEntryId, maxItemsCount);
        }

    	/// <summary>
    	/// Receives the update pakage.
    	/// </summary>
    	/// <param name="updatePackage"></param>
    	public void ReceiveUpdatePackage(UpdatePackage updatePackage)
    	{
            try
            {
                UpdateStarted.SafeInvoke(this);
                updateReceiver.SaveUpdatePackage(updatePackage);
            }
            finally
            {
                UpdateFinished.SafeInvoke(this);
            }
    	}



        /// <summary>
        /// Occurs when an update is started.
        /// </summary>
        public event EventHandler UpdateStarted;

        /// <summary>
        /// Occurs when an update is finished.
        /// </summary>
        public event EventHandler UpdateFinished;

    	/// <summary>
        /// Pings the tracking side
        /// </summary>
        /// <param name="pingInterval"></param>
        /// <returns>true if everything is ok</returns>
        public PingResult Ping(TimeSpan pingInterval)
        {
            var @event = PingReceived;
            if (@event != null)
                @event(this, new EventArgs<TimeSpan>(pingInterval));

            return new PingResult { 
				Name = Name, 
				Version = Version};
        }

        /// <summary>
        /// Occurs when the instance is pinged.
        /// </summary>
        public event EventHandler<EventArgs<TimeSpan>> PingReceived;
    }
}