using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NDistribUnit.Common.HashChecks;
using NDistribUnit.Common.Logging;

namespace NDistribUnit.Common.Updating
{
	/// <summary>
	/// 
	/// </summary>
	public class VersionDirectoryFinder
	{
		private ILog log;
		/// <summary>
		/// 
		/// </summary>
		public static readonly Regex VersionPattern = new Regex(@"^(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+)\.(?<revision>\d+)$", RegexOptions.Compiled);

		/// <summary>
		/// Initializes a new instance of the <see cref="VersionDirectoryFinder"/> class.
		/// </summary>
		/// <param name="log">The log.</param>
		public VersionDirectoryFinder(ILog log)
		{
			this.log = log;
		}

		/// <summary>
		/// Gets the version directory.
		/// </summary>
		/// <param name="directoryName">Name of the directory.</param>
		/// <returns></returns>
		public DirectoryInfo GetVersionDirectory(string directoryName)
		{
			var directory = new DirectoryInfo(directoryName);
			if (!directory.Exists)
				throw new InvalidOperationException();

			var subDirectories = (from subDir in directory.GetDirectories()
			                      let match = VersionPattern.Match(subDir.Name)
			                      where match.Success
			                      orderby
			                      	int.Parse(match.Groups["major"].Value),
			                      	int.Parse(match.Groups["minor"].Value),
			                      	int.Parse(match.Groups["build"].Value),
			                      	int.Parse(match.Groups["revision"].Value)
			                      select subDir).ToList();

			for (int i = subDirectories.Count - 1; i >= 0; i--)
			{
				var subDirectory = subDirectories[i];
				var hash = new DirectoryHash(subDirectory.FullName);
				try
				{
					hash.Validate();
					return subDirectory;
				}
				catch (HashValidationException ex)
				{
					log.Info(string.Format("Error, when trying to validate the directory '{0}':", subDirectory.FullName));
					log.Info(ex.Message);
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the version directory.
		/// </summary>
		/// <param name="directoryName">Name of the directory.</param>
		/// <param name="version">The version.</param>
		public DirectoryInfo GetVersionDirectory(string directoryName, Version version)
		{
			var versionDirectory = new DirectoryInfo(directoryName).GetDirectories().FirstOrDefault(directory => VersionPattern.IsMatch(directory.Name) && new Version(directory.Name) == version);

			if (versionDirectory == null)
				return null;

			try
			{
				new DirectoryHash(versionDirectory.FullName).Validate();
				return versionDirectory;
			}
			catch(HashValidationException)
			{
				return null;
			}
			
		}
	}
}