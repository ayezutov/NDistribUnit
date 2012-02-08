using System;
using NDistribUnit.Common.Agent.Naming;
using NDistribUnit.Common.Common.ConsoleProcessing;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Common.Agent
{
    /// <summary>
    /// The entry point into agent's console application
    /// </summary>
    public class AgentProgram : GeneralProgram
    {
        private readonly IInstanceTracker instanceTracker;

        /// <summary>
        ///  The host, which enables all communication services
        /// </summary>
        public AgentHost AgentHost { get; set; }


        /// <summary>
        /// Initializes a new instance of an agent program
        /// </summary>
        /// <param name="agentHost">The agent host.</param>
        /// <param name="bootstrapperParameters">The bootstrapper parameters.</param>
        /// <param name="updatesMonitor">The updates availability monitor.</param>
        /// <param name="exceptionCatcher">The exception catcher.</param>
        /// <param name="instanceTracker">The instance tracker.</param>
        /// <param name="log">The log.</param>
        public AgentProgram(AgentHost agentHost, 
            BootstrapperParameters bootstrapperParameters,
            UpdatesMonitor updatesMonitor, 
            ExceptionCatcher exceptionCatcher,
            IInstanceTracker instanceTracker,
            ILog log) : base(updatesMonitor, exceptionCatcher, log, bootstrapperParameters)
        {
            this.instanceTracker = instanceTracker;
            AgentHost = agentHost;
        }


        /// <summary>
        /// Runs this instance.
        /// </summary>
        /// <returns></returns>
        public int Run()
        {
//            var watcher = new ProcessCreationWatcher();
//            watcher.Start();
//
//            new NUnitInitializer().Initialize();
//
//            var p = new TestPackage(@"d:\work\personal\NDistribUnit\test\FOCS-2012-01-30\FOCS.UI.Tests.dll");
//            p.Settings["RuntimeFramework"] = RuntimeFramework.Parse("net-2.0");
//            var pr = new NDistribUnitProcessRunner();
//            pr.Load(p);
//            var agent = pr.Agent;
//            var processId = 0;
//            if (agent != null)
//            {
//                processId = (agent as RemoteTestAgent).ProcessId;
//            }
//
//            System.Console.WriteLine(processId);
//            
//            pr.Unload();
//            pr.Dispose();
//            var s = System.Console.ReadLine();

//            return string.IsNullOrEmpty(s) ? 1: 0;

            return exceptionCatcher.Run(
                () =>
                    {
                        System.Console.WriteLine("Program point: {0}", instanceTracker.GetInstanceNumber());
                        //AgentHost.LoadState();
                        updatesMonitor.Start();

                        try
                        {
                            log.BeginActivity("Starting agent...");
                            AgentHost.Start();
                            log.EndActivity("Agent was successfully started. Please type \"exit\" and press <Enter> to exit.");
                        }
                        catch (Exception ex)
                        {
                            log.Error("Exception, while starting agent", ex);
                            throw;
                        }

                        var returnCode = WaitAndGetReturnCode(new string[0] /*{bootstrapperParameters.ConfigurationFile}*/);

                        updatesMonitor.Stop();
                        // AgentHost.SaveState();
                        AgentHost.Stop();


                        return returnCode;
                    });
        }
    }
}