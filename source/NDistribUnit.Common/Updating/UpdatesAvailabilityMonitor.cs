﻿using System;
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
	public class UpdatesAvailabilityMonitor
	{
		private readonly string path;
		private FileSystemWatcher fileSystemWatcher;
		private readonly Timer timer;
		private readonly Regex versionPattern = new Regex(@"(?<=[\\\/])(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+)\.(?<revision>\d+)(?=([\\\/]|$))", RegexOptions.Compiled);
		private readonly ILog log;
		private readonly IUpdater updater;

		/// <summary>
		/// Initializes a new instance of the <see cref="UpdatesAvailabilityMonitor"/> class.
		/// </summary>
		public UpdatesAvailabilityMonitor(ILog log, IUpdater updater, BootstrapperParameters bootstrapperParameters)
		{
			path = Path.GetDirectoryName(bootstrapperParameters.BootstrapperFile);
			this.log = log;
			this.updater = updater;

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

			if (IsVersionHigherThanCurrent(versionDirectory.Name) && updater != null)
			{ 
				log.Success(string.Format("Updating to version: {0}", versionDirectory.Name));
				updater.PerformUpdate();
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

		private static bool IsVersionHigherThanCurrent(string versionString)
		{
			var version = new Version(versionString);
			var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

			return currentVersion < version;
		}

		private string GetVersionString(string fullPath)
		{
			var match = versionPattern.Match(fullPath.Replace(path, string.Empty));
			return match.Success ? match.Value : null;
		}
	}
}