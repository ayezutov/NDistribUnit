using System.IO;
using Ionic.Zip;

namespace NDistribUnit.Common.Communication
{
	/// <summary>
	/// 
	/// </summary>
	public class ZipSource
	{
		/// <summary>
		/// Gets the zipped bytes.
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <returns></returns>
		public byte[] GetZippedBytes(DirectoryInfo directory)
		{
			if (directory == null)
				return null;

			using (var zipFile = new ZipFile())
			{
				zipFile.AddDirectory(directory.FullName, directory.Name);
				using (var stream = new MemoryStream())
				{
					zipFile.Save(stream);
					return stream.ToArray();
				}
			}
		}
	}
}
