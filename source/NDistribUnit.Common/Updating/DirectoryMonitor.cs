using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using NDistribUnit.Common.Logging;

namespace NDistribUnit.Common.Updating
{
	/// <summary>
	/// Monitors the directory to see, if a newer version of the program was placed besides
	/// </summary>
	public class DirectoryMonitor
	{
		private readonly string path;
		private readonly FileSystemWatcher fileSystemWatcher;
		private readonly Timer timer;
		private readonly Regex versionPattern = new Regex(@"(?<=[\\\/])(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+)\.(?<revision>\d+)(?=([\\\/]|$))", RegexOptions.Compiled);
		private readonly ILog log;

		/// <summary>
		/// Occurs when update possibility is detected.
		/// </summary>
		public event Action UpdatePossibilityDetected;

		/// <summary>
		/// Initializes a new instance of the <see cref="DirectoryMonitor"/> class.
		/// </summary>
		public DirectoryMonitor(ILog log, string path)
		{
			this.path = path;
			this.log = log;


			fileSystemWatcher = new FileSystemWatcher(path)
			{
				IncludeSubdirectories = true,
				EnableRaisingEvents = false,
				NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.Size
			};
			fileSystemWatcher.Changed += FileSystemWatcherChanged;
			fileSystemWatcher.Created += FileSystemWatcherChanged;
			fileSystemWatcher.Renamed += FileSystemWatcherChanged;

			timer = new Timer(SomeFileWasChangedAfterWait);
		}

		/// <summary>
		/// Starts this instance.
		/// </summary>
		public void Start()
		{
			fileSystemWatcher.EnableRaisingEvents = true;
		}

		private void SomeFileWasChangedAfterWait(object state)
		{
			var finder = new VersionDirectoryFinder(log);
			var versionDirectory = finder.GetVersionDirectory(Path.GetDirectoryName(path));
			if (versionDirectory == null)
				return;

			var lastVersion = new Version(versionDirectory.Name);
			if (lastVersion > Assembly.GetExecutingAssembly().GetName().Version)
			{
				var method = UpdatePossibilityDetected;
				if (method != null)
					method();
			}
		}

		private void FileSystemWatcherChanged(object sender, FileSystemEventArgs e)
		{
			var versionString = GetVersionString(e.FullPath);
			if (string.IsNullOrEmpty(versionString))
				return;
			var version = new Version(versionString);
			var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

			if (currentVersion < version)
				timer.Change(500, Timeout.Infinite);
		}

		private string GetVersionString(string fullPath)
		{
			var match = versionPattern.Match(fullPath.Replace(path, string.Empty));
			return match.Success ? match.Value : null;
		}
	}
}