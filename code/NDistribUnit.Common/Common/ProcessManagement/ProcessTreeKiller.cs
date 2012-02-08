using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using WMI.Win32;
using System.Linq;

namespace NDistribUnit.Common.Common.ProcessManagement
{
    /// <summary>
    /// 
    /// </summary>
    public class ProcessTreeKiller
    {
        private readonly IEnumerable<Win32_Process> processes;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessTreeKiller"/> class.
        /// </summary>
        public ProcessTreeKiller(): this(GetAllProcesses())
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessTreeKiller"/> class.
        /// </summary>
        /// <param name="processes">The processes.</param>
        public ProcessTreeKiller(IEnumerable<Win32_Process> processes)
        {
            this.processes = processes;
        }

        /// <summary>
        /// Kills the tree.
        /// </summary>
        public void KillTree(uint treeRoot, bool killRoot = true)
        {
            var groups = processes.GroupBy(p => p.ParentProcessId);

            KillTree(groups, treeRoot, killRoot);
        }

        private void KillTree(IEnumerable<IGrouping<uint, Win32_Process>> groupedProcesses, uint root, bool killRoot)
        {
            var children = groupedProcesses.FirstOrDefault(g => g.Key.Equals(root));

            if (children != null)
            {
                foreach (Win32_Process child in children)
                {
                    KillTree(groupedProcesses, child.ProcessId, true);
                }
            }

            if (killRoot)
            {
                try
                {
                    var process = Process.GetProcessById((int) root);
                    process.Kill();
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Gets all processes.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Win32_Process> GetAllProcesses()
        {
            using (var searcher = new ManagementObjectSearcher(new SelectQuery("Win32_Process")))
            {
                using (ManagementObjectCollection results = searcher.Get())
                {
                    return (from ManagementBaseObject p in results select new Win32_Process(p)).ToList();
                }
            }

        }
    }
}