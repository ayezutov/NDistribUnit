using System;
using System.ServiceModel;
using NDistribUnit.Agent.Naming;
using NDistribUnit.Common;
using NDistribUnit.Common.Communication.ConnectionTracking;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Agent.Communication
{
    /// <summary>
    /// The service, which is communicated, when the server calls he agent
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class TestRunnerAgent : ITestRunnerAgent
    {
        private readonly ILog log;
        private readonly INameProvider nameProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunnerAgent"/> class.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="nameProvider">The name provider.</param>
        public TestRunnerAgent(ILog log, INameProvider nameProvider)
        {
            this.log = log;
            this.nameProvider = nameProvider;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name
        {
            get { return nameProvider.Name; }
        }

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
        /// Pings the tracking side
        /// </summary>
        /// <param name="pingInterval"></param>
        /// <returns>true if everything is ok</returns>
        public bool Ping(TimeSpan pingInterval)
        {
            var @event = PingReceived;
            if (@event != null)
                @event(this, new EventArgs<TimeSpan>(pingInterval));
            return true;
        }

        /// <summary>
        /// Occurs when the instance is pinged.
        /// </summary>
        public event EventHandler<EventArgs<TimeSpan>> PingReceived;
    }
}