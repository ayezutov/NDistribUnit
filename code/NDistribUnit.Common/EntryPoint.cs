using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using NDistribUnit.Common.Agent.Naming;
using NDistribUnit.Common.Common;
using NDistribUnit.Common.Common.Cleanup;
using NDistribUnit.Common.Common.Domains;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Agent
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class EntryPoint
    {
        private const string appDomainMergedConfigurationKey = "ndistribunit.bootstrapped.configuration.merged";
        private const string appDomainInstanceTrackerState = "ndistribunit.bootstrapped.instanceTracker.State";

        /// <summary>
        /// Runs the specified program with provided arguments.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        protected int Run(string[] args)
        {
            var bootstrapperParameters = BootstrapperParameters.InitFromDomain(AppDomain.CurrentDomain);
            if (!bootstrapperParameters.AllParametersAreFilled)
            {
                Console.WriteLine("This programm cannot be launched directly");
                return (int) ReturnCodes.CannotLaunchBootstrappedApplicationDirectly;
            }

            var state = AppDomain.CurrentDomain.GetData(appDomainInstanceTrackerState) as InstanceTrackerState;
            if (state != null)
                InstanceTracker.InitToState(state);

            var startupCleaner = new StartupCleaner(bootstrapperParameters.RootFolder);
            var instanceTracker = new InstanceTracker();
            instanceTracker.Init(i=> startupCleaner.RunOnFirstInstanceStart());

            System.Console.WriteLine("Entry point: {0}", instanceTracker.GetInstanceNumber());
            startupCleaner.Run();

            var mergedConfigFileName = (string)AppDomain.CurrentDomain.GetData(appDomainMergedConfigurationKey);

            if (bootstrapperParameters.ConfigurationFile != null && mergedConfigFileName == null)
            {
                string entryAssemblyFile = Assembly.GetEntryAssembly().Location;
                var mergedConfig = new ConfigurationFileMerger()
                    .MergeFiles(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
                                bootstrapperParameters.ConfigurationFile, instanceTracker.GetInstanceNumber().ToString(CultureInfo.InvariantCulture));

                var domain = DomainManager.CreateDomain(
                    AppDomain.CurrentDomain.FriendlyName + "-with-merged-configuration", 
                    Path.GetDirectoryName(entryAssemblyFile),
                    mergedConfig);
                
                DomainManager.AddResolverForPaths(domain, DomainManager.GetNUnitFolders(bootstrapperParameters));
                try
                {
                    BootstrapperParameters.WriteToDomain(bootstrapperParameters, domain);
                    domain.SetData(appDomainMergedConfigurationKey, mergedConfig);
                    domain.SetData(appDomainInstanceTrackerState, InstanceTracker.GetState());
                    return domain.ExecuteAssembly(entryAssemblyFile, args);
                }
                catch
                {
                    DomainManager.UnloadDomain(domain);
                    throw;
                }
            }
            
            if (bootstrapperParameters.ConfigurationFile == null && mergedConfigFileName == null)
            {
                DomainManager.AddResolverForPaths(AppDomain.CurrentDomain, DomainManager.GetNUnitFolders(bootstrapperParameters));
            }

            return RunProgram(args);
        }

        /// <summary>
        /// Runs the program specific code.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        protected abstract int RunProgram(string[] args);
    }
}