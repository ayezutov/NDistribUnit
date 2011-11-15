using System;
using System.ServiceModel;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Common.Logging;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Common.TestExecution;
using TestResult = NDistribUnit.Common.Contracts.DataContracts.TestResult;

namespace NDistribUnit.Common.Agent
{
    /// <summary>
    /// The service, which is communicated, when the server calls he agent
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class Agent : IAgent, IRemoteAppPart
    {
        private readonly ILog log;
    	private readonly RollingLog logStorage;
		private readonly IUpdateReceiver updateReceiver;
        private readonly IVersionProvider versionProvider;
        private readonly IConnectionProvider connectionProvider;
        private readonly AgentTestRunner runner;

        /// <summary>
        /// Initializes a new instance of the <see cref="Agent"/> class.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="logStorage">The log storage.</param>
        /// <param name="updateReceiver">The update receiver.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="versionProvider">The version provider.</param>
        /// <param name="connectionProvider">The connection provider.</param>
        /// <param name="runner">The runner.</param>
        public Agent(
            ILog log, 
            RollingLog logStorage, 
            IUpdateReceiver updateReceiver, 
            AgentConfiguration configuration, 
            IVersionProvider versionProvider, 
            IConnectionProvider connectionProvider, 
            AgentTestRunner runner)
        {
            this.log = log;
			this.logStorage = logStorage;
    		this.updateReceiver = updateReceiver;
            this.versionProvider = versionProvider;
            this.connectionProvider = connectionProvider;
            this.runner = runner;
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
        /// <returns></returns>
        public TestResult RunTests(TestUnit test)
        {
            log.Info(string.Format("Run Tests command Received: {0}", test));

            var dataSource = connectionProvider.GetCurrentCallback<IAgentDataSource>();

            return runner.Run(test, dataSource);
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
    		updateReceiver.SaveUpdatePackage(updatePackage);
    	}


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
				EndpointName = Name, 
				Version = Version};
        }

        /// <summary>
        /// Occurs when the instance is pinged.
        /// </summary>
        public event EventHandler<EventArgs<TimeSpan>> PingReceived;
    }
}