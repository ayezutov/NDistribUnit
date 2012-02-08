using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using NDistribUnit.Common.Common.ProcessManagement;
using WMI.Win32;

namespace NDistribUnit.Common.Common.Cleanup
{
    /// <summary>
    /// Performs clean up operations on program startup
    /// </summary>
    public class StartupCleaner
    {
        private readonly string rootPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupCleaner"/> class.
        /// </summary>
        /// <param name="rootPath">The root path.</param>
        public StartupCleaner(string rootPath)
        {
            this.rootPath = rootPath.ToUpperInvariant();
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        public void Run()
        {
            var processes = ProcessTreeKiller.GetAllProcesses();
            var orphanedProcessesFromSubFolders = GetOrphanedProcessesFromSubfolders(processes);

            var processTreeKiller = new ProcessTreeKiller(processes);
            
            foreach (Win32_Process orphanedProcess in orphanedProcessesFromSubFolders)
            {
                processTreeKiller.KillTree(orphanedProcess.ProcessId);
            }
        }

        private IEnumerable<Win32_Process> GetOrphanedProcessesFromSubfolders(IEnumerable<Win32_Process> processes)
        {
            var orphanedProcessesFromSubFolders = (from process in processes
                                                   let executablePath =
                                                       process.ExecutablePath != null
                                                           ? process.ExecutablePath.ToUpperInvariant()
                                                           : null
                                                   where
                                                       !string.IsNullOrEmpty(executablePath) &&
                                                       executablePath.StartsWith(rootPath) &&
                                                       !Path.GetDirectoryName(executablePath).Equals(rootPath)
                                                   let parent =
                                                       processes.FirstOrDefault(p => p.ProcessId.Equals(process.ParentProcessId))
                                                   where parent == null
                                                   select process);

            return orphanedProcessesFromSubFolders;
        }

        

        /// <summary>
        /// Runs the on first instance start.
        /// </summary>
        public void RunOnFirstInstanceStart()
        {
            System.Console.WriteLine("First instance cleanup");
        }
    }
}