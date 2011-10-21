using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Common.Agent
{
    /// <summary>
    /// A host, which manages all agent's services
    /// </summary>
    public class AgentHost
    {
        private readonly IEnumerable<IAgentExternalModule> modules;
        private ILog log;
        internal ServiceHost TestRunnerHost { get; set; }

        /// <summary>
        /// Gets the endpoint.
        /// </summary>
        public ServiceEndpoint Endpoint { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentHost"/> class.
        /// </summary>
        /// <param name="testRunner">The test runner.</param>
        /// <param name="modules">The agents' external modules.</param>
        /// <param name="log">The log.</param>
        public AgentHost(TestRunnerAgent testRunner, IEnumerable<IAgentExternalModule> modules, ILog log)
        {
            TestRunner = testRunner;
            this.modules = modules;
            this.log = log;
        }

        /// <summary>
        /// Starts agent's services
        /// </summary>
        public void Start()
        {
            var baseAddress = new Uri(string.Format("net.tcp://{0}:{1}", Environment.MachineName, WcfUtilities.FindPort()));

            log.BeginActivity(string.Format("Starting agent {1} on '{0}'...", baseAddress, TestRunner.Name));
            TestRunnerHost = new ServiceHost(TestRunner, baseAddress);
			Endpoint = TestRunnerHost.AddServiceEndpoint(typeof(ITestRunnerAgent), new NetTcpBinding("NDistribUnit.Default"), "");
        	TestRunnerHost.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = true;
            log.BeginActivity("Starting external modules...");
            foreach (var module in modules)
            {
                module.Start(this);
            }
            log.EndActivity("External modules were started.");

            TestRunnerHost.Open();
            log.EndActivity("Agent host was started.");
        }

        /// <summary>
        /// Gets or sets the test runner.
        /// </summary>
        /// <value>
        /// The test runner.
        /// </value>
        public TestRunnerAgent TestRunner { get; set; }

        /// <summary>
        /// Stops all agent's services
        /// </summary>
        public void Stop()
        {
            log.BeginActivity("Stopping host...");
            TestRunnerHost.Close();
            log.BeginActivity("Stopping external modules...");
            foreach (var module in modules)
            {
                module.Stop(this);
            }
            log.EndActivity("External modules were stopped.");
            log.EndActivity("Agent host was stopped");
        }

        /// <summary>
        /// Aborts this instance.
        /// </summary>
        public void Abort()
        {
            TestRunnerHost.Abort();
            foreach (var module in modules)
            {
                module.Abort(this);
            }
        }
    }
}