using System;
using NDistribUnit.Common.Common.ProcessManagement;
using NDistribUnit.Common.Logging;
using NUnit.Core;
using NUnit.Util;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public class NDistribUnitProcessRunner : ProxyTestRunner
    {
        private readonly ILog log;
        private TestAgent agent;

        private RuntimeFramework runtimeFramework;

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="NDistribUnitProcessRunner"/> class.
        /// </summary>
        public NDistribUnitProcessRunner(ILog log) : base(0)
        {
            this.log = log;
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets the runtime framework.
        /// </summary>
        public RuntimeFramework RuntimeFramework
        {
            get { return runtimeFramework; }
        }
        #endregion

        /// <summary>
        /// Gets the agent.
        /// </summary>
        public RemoteTestAgent Agent
        {
            get { return agent as RemoteTestAgent; }
        }

        /// <summary>
        /// Loads the specified package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns></returns>
        public override bool Load(TestPackage package)
        {
            //                log.Info("Loading " + package.Name);
            Unload();

            runtimeFramework = package.Settings["RuntimeFramework"] as RuntimeFramework;
            if (runtimeFramework == null)
                runtimeFramework = RuntimeFramework.CurrentFramework;

            bool enableDebug = package.GetSetting("EnableDebug", false);

            bool loaded = false;

            try
            {
                if (this.agent == null)
                {
                    this.agent = Services.TestAgency.GetAgent(
                        runtimeFramework,
                        30000,
                        enableDebug);

                    if (this.agent == null)
                        return false;
                }

                if (this.TestRunner == null)
                    this.TestRunner = agent.CreateRunner(this.runnerID);

                loaded = base.Load(package);
                return loaded;
            }
            finally
            {
                // Clean up if the load failed
                if (!loaded) Unload();
            }
        }

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        public override void Unload()
        {
            if (Test != null)
            {
                //                    log.Info("Unloading " + Path.GetFileName(Test.TestName.Name));
                this.TestRunner.Unload();
                this.TestRunner = null;
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public override void Dispose()
        {
            // Do this first, because the next step will
            // make the downstream runner inaccessible.
            base.Dispose();

            if (this.agent != null)
            {
                //                    log.Info("Stopping remote agent");
                agent.Stop();
                this.agent = null;
            }
        }

        #endregion

        /// <summary>
        /// Cleanups the after run.
        /// </summary>
        public void CleanupAfterRun()
        {
            try
            {
                if (Agent != null)
                {
                    new ProcessTreeKiller().KillTree((uint)Agent.ProcessId, false);
                }
            }
            catch(Exception ex)
            {
                log.Warning("Exception while cleaning up resources after run", ex);
            }
        }
    }
}