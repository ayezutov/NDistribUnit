using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using NDistribUnit.Common.Common.Networking;
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
        /// <summary>
        /// The relative address of the IRemoteParticle interface (the non-duplex)
        /// </summary>
        public static readonly string RemoteParticleAddress = "remote";

        private readonly IEnumerable<IAgentExternalModule> modules;
        private readonly AgentConfiguration configuration;

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
        /// <param name="configuration">The configuration.</param>
        /// <param name="log">The log.</param>
        public AgentHost(Agent testRunner, IEnumerable<IAgentExternalModule> modules, AgentConfiguration configuration,  ILog log)
        {
            TestRunner = testRunner;
            this.modules = modules;
            this.configuration = configuration;
            this.log = log;
        }

        /// <summary>
        /// Starts agent's services
        /// </summary>
        public void Start()
        {
            var port = configuration.Port;
            if (!port.HasValue || !WcfUtilities.IsFreePort(port.Value))
                port = WcfUtilities.FindPort();

            var baseAddress = new Uri(string.Format("net.tcp://{0}:{1}", Environment.MachineName, port));

            log.BeginActivity(string.Format("Starting agent {1} on '{0}'...", baseAddress, TestRunner.Name));
            TestRunnerHost = new ServiceHost(TestRunner, baseAddress);
            
			Endpoint = TestRunnerHost.AddServiceEndpoint(typeof(IAgent), new NetTcpBinding("NDistribUnit.Default"), "");
            TestRunnerHost.AddServiceEndpoint(typeof(IRemoteAppPart), new NetTcpBinding("NDistribUnit.Default"), RemoteParticleAddress);
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
        public Agent TestRunner { get; set; }

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