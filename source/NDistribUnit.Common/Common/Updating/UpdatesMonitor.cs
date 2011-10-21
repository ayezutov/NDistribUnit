using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Common.Common.Updating
{
	/// <summary>
	/// Monitors the directory to see, if a newer version of the program was placed besides
	/// </summary>
	public class UpdatesMonitor
	{
		private readonly string path;
		private Guid guid = Guid.NewGuid();
		private FileSystemWatcher fileSystemWatcher;
		private readonly Timer timer;
		private readonly Regex versionPattern = new Regex(@"(?<=[\\\/])(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+)\.(?<revision>\d+)(?=([\\\/]|$))", RegexOptions.Compiled);
		private readonly ILog log;
	    private readonly IVersionProvider versionProvider;

	    /// <summary>
		/// Occurs when an update is available.
		/// </summary>
		public event Action UpdateAvailable;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatesMonitor"/> class.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="bootstrapperParameters">The bootstrapper parameters.</param>
        /// <param name="versionProvider">The version provider.</param>
		public UpdatesMonitor(ILog log, BootstrapperParameters bootstrapperParameters, IVersionProvider versionProvider)
		{
			path = Path.GetDirectoryName(bootstrapperParameters.BootstrapperFile);
			this.log = log;
            this.versionProvider = versionProvider;

            timer = new Timer(SomeFileWasChangedAfterWait);
		}

		/// <summary>
		/// Starts this instance.
		/// </summary>
		public void Start()
		{
			fileSystemWatcher = new FileSystemWatcher(path)
			{
				IncludeSubdirectories = true,
				EnableRaisingEvents = true,
				NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.Size
			};
			fileSystemWatcher.Changed += FileSystemWatcherChanged;
			fileSystemWatcher.Created += FileSystemWatcherChanged;
			fileSystemWatcher.Renamed += FileSystemWatcherChanged;
		}

		private void SomeFileWasChangedAfterWait(object state)
		{
			var finder = new VersionDirectoryFinder(log);

			DirectoryInfo versionDirectory = finder.GetVersionDirectory(path);
			
			if (versionDirectory == null)
				return;

			if (IsVersionHigherThanCurrent(versionDirectory.Name) && UpdateAvailable != null)
			{
				log.Success(string.Format("Updating to version: {0}, Thread: {1}, Guid: {2}", versionDirectory.Name, Thread.CurrentThread.ManagedThreadId, guid));
				UpdateAvailable();
			}
		}

		private void FileSystemWatcherChanged(object sender, FileSystemEventArgs e)
		{
			var versionString = GetVersionString(e.FullPath);
			if (string.IsNullOrEmpty(versionString))
				return;

			if (IsVersionHigherThanCurrent(versionString))
			{
				timer.Change(1000, Timeout.Infinite);
			}
		}

		private bool IsVersionHigherThanCurrent(string versionString)
		{
			var version = new Version(versionString);
			var currentVersion = versionProvider.GetVersion();

			return currentVersion < version;
		}

		private string GetVersionString(string fullPath)
		{
			var match = versionPattern.Match(fullPath.Replace(path, string.Empty));
			return match.Success ? match.Value : null;
		}

		/// <summary>
		/// Stops this instance.
		/// </summary>
		public void Stop()
		{
			fileSystemWatcher.EnableRaisingEvents = false;
			fileSystemWatcher.Dispose();
		}
	}
}