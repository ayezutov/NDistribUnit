using System;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using NDistribUnit.Common.Agent.Naming;
using NDistribUnit.Common.Common.ConsoleProcessing;
using NDistribUnit.Common.Common.Logging;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Logging;
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
        private readonly ExceptionCatcher exceptionCatcher;
        private readonly IProjectsStorage projects;

        /// <summary>
        /// Initializes a new instance of the <see cref="Agent"/> class.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="logStorage">The log storage.</param>
        /// <param name="updateReceiver">The update receiver.</param>
        /// <param name="versionProvider">The version provider.</param>
        /// <param name="runner">The runner.</param>
        /// <param name="exceptionCatcher">The exception catcher.</param>
        /// <param name="projects">The projects.</param>
        /// <param name="instanceProvider">The instance provider.</param>
        public Agent(
            ILog log,
            RollingLog logStorage,
            IUpdateReceiver updateReceiver,
            IVersionProvider versionProvider,
            AgentTestRunner runner,
            ExceptionCatcher exceptionCatcher,
            IProjectsStorage projects,
            IInstanceTracker instanceProvider)
        {
            this.log = log;
            this.logStorage = logStorage;
            this.updateReceiver = updateReceiver;
            this.versionProvider = versionProvider;
            this.runner = runner;
            this.exceptionCatcher = exceptionCatcher;
            this.projects = projects;
            Name = string.Format("{0} #{1:000}", Environment.MachineName, instanceProvider.GetInstanceNumber());
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
            return ExecuteWithEvents(() =>
            {
                log.Info(string.Format("Run Tests command Received: {0}", test.UniqueTestId));

                TestResult result = null;

                exceptionCatcher.Run(() =>
                                         {
                                             result = runner.Run(test, configurationSubstitutions);
                                             result.ForSelfAndAllDescedants(r => r.SetAgentName(Name));
                                         });
                return result;
            });
        }

        /// <summary>
        /// Releases the resources.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        public void ReleaseResources(TestRun testRun)
        {
            ExecuteWithEvents(() =>
            {
                log.BeginActivity(string.Format("Releasing resources for test run {0}", testRun));
                exceptionCatcher.Run(() =>
                                         {
                                             runner.Unload(testRun);
                                             if (!testRun.IsAliasedTest)
                                             {
                                                 projects.RemoveProject(testRun);
                                             }
                                         });
                log.EndActivity(string.Format("Finished releasing resources for test run {0}", testRun));
            });
        }

        /// <summary>
        /// Receives the project.
        /// </summary>
        /// <param name="project">The project.</param>
        public void ReceiveProject(ProjectMessage project)
        {
            ExecuteWithEvents(() =>
            {
                log.BeginActivity(string.Format("Receiving project for test: {0} ({1})", project.TestRun.Id, project.TestRun.NUnitParameters.AssembliesToTest[0]));
                projects.Store(project.TestRun, project.Project);
                log.EndActivity(string.Format("Received project for test: {0} ({1})", project.TestRun.Id, project.TestRun.NUnitParameters.AssembliesToTest[0]));
            });
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
            return ExecuteWithEvents(() => projects.HasProject(run));
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
            ExecuteWithEvents(() => updateReceiver.SaveUpdatePackage(updatePackage));
        }

        private void ExecuteWithEvents(Action action)
        {
            ExecuteWithEvents(() =>
                                  {
                                      action();
                                      return true;
                                  });
        }

        private TResult ExecuteWithEvents<TResult>(Func<TResult> func)
        {
            try
            {
                CommunicationStarted.SafeInvoke(this);
                return func();
            }
            finally
            {
                CommunicationFinished.SafeInvoke(this);
            }
        }

        /// <summary>
        /// Occurs when an update is started.
        /// </summary>
        public event EventHandler CommunicationStarted;

        /// <summary>
        /// Occurs when an update is finished.
        /// </summary>
        public event EventHandler CommunicationFinished;

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

            return new PingResult
            {
                Name = Name,
                Version = Version
            };
        }

        /// <summary>
        /// Occurs when the instance is pinged.
        /// </summary>
        public event EventHandler<EventArgs<TimeSpan>> PingReceived;
    }
}