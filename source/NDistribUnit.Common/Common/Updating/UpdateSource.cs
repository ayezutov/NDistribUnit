using System;
using System.IO;
using System.Reflection;
using Ionic.Zip;
using NDistribUnit.Common.Communication;

namespace NDistribUnit.Common.Updating
{
	/// <summary>
	/// 
	/// </summary>
	public class UpdateSource: IUpdateSource
	{
		private readonly ZipSource zip;

		/// <summary>
		/// Initializes a new instance of the <see cref="UpdateSource"/> class.
		/// </summary>
		/// <param name="zip">The zip.</param>
		public UpdateSource(ZipSource zip)
		{
			this.zip = zip;
		}

		/// <summary>
		/// Gets the zipped version folder.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <returns></returns>
		public byte[] GetZippedVersionFolder(Version version)
		{
			var directory = new DirectoryInfo(Path.GetDirectoryName(new Uri(Assembly.GetEntryAssembly().CodeBase).AbsolutePath));

			while (directory != null && !VersionDirectoryFinder.VersionPattern.IsMatch(directory.Name))
			{
				directory = directory.Parent;
			}

			return zip.GetZippedBytes(directory);
		}
	}
}