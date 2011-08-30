using System;
using System.IO;
using System.Reflection;
using Ionic.Zip;

namespace NDistribUnit.Common.Updating
{
	/// <summary>
	/// 
	/// </summary>
	public class UpdateSource: IUpdateSource
	{
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

			if (directory == null)
				return null;

			using (var zipFile = new ZipFile())
			{
				zipFile.AddDirectory(directory.FullName, directory.Name);
				//zipFile.AddSelectedFiles("*.exe", directory.Parent.FullName, "", false);
				using (var stream = new MemoryStream())
				{
					zipFile.Save(stream);
					return stream.ToArray();
				}
			}

		}
	}
}