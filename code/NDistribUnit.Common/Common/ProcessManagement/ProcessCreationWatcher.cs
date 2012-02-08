using System;
using System.Management;
using WMI.Win32;
using NDistribUnit.Common.Extensions;

namespace NDistribUnit.Common.Common.ProcessManagement
{
    /// <summary>
    /// 
    /// </summary>
    public class ProcessCreationWatcher 
    {
        // WMI WQL process query strings
        private const string WmiProcessCreationQuery = @"SELECT * FROM 
__InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process'";

        /// <summary>
        /// Occurs when a process is created.
        /// </summary>
        public event EventHandler<EventArgs<Win32_Process>> ProcessCreated;

        private ManagementEventWatcher watcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessCreationWatcher"/> class.
        /// </summary>
        public ProcessCreationWatcher()
        { 
            watcher = new ManagementEventWatcher(new WqlEventQuery(WmiProcessCreationQuery));
            watcher.EventArrived += WatcherEventArrived;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            watcher.Start();
        }

        private void WatcherEventArrived(object sender, EventArrivedEventArgs e)
        {
            string eventType = e.NewEvent.ClassPath.ClassName;
            System.Console.WriteLine("Process "+eventType);
            var proc = new Win32_Process(e.NewEvent["TargetInstance"] as ManagementBaseObject);

            ProcessCreated.SafeInvoke(this, proc);
        }
    }
}