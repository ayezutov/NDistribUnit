using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Common.Agent
{
    /// <summary>
    /// The service, which is communicated, when the server calls he agent
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class TestRunnerAgentService : ITestRunnerAgent
    {
        private readonly ILog log;
        private readonly RollingLog logStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunnerAgentService"/> class.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="name">The name of the agent.</param>
        /// <param name="logStorage">The log storage.</param>
        public TestRunnerAgentService(ILog log, string name, RollingLog logStorage)
        {
            this.log = log;
            this.logStorage = logStorage;
            Name = name;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Runs tests on agent
        /// </summary>
        /// <param name="callbackValue"></param>
        /// <returns></returns>
        public bool RunTests(string callbackValue)
        {
            log.Info(string.Format("Run Tests command Received: {0}", callbackValue));
            return true;
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
        /// Pings the tracking side
        /// </summary>
        /// <param name="pingInterval"></param>
        /// <returns>true if everything is ok</returns>
        public PingResult Ping(TimeSpan pingInterval)
        {
            var @event = PingReceived;
            if (@event != null)
                @event(this, new EventArgs<TimeSpan>(pingInterval));

            return new PingResult { AgentState = AgentState.Connected, EndpointName = Name};
        }

        /// <summary>
        /// Occurs when the instance is pinged.
        /// </summary>
        public event EventHandler<EventArgs<TimeSpan>> PingReceived;
    }
}