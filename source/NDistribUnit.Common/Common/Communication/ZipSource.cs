using System.IO;
using Ionic.Zip;

namespace NDistribUnit.Common.Common.Communication
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
        /// <param name="zipContentOnly">if set to <c>true</c> places the content into the root folder, otherwise </param>
        /// <returns></returns>
        public byte[] GetPackedFolder(string directory, bool zipContentOnly = false)
		{
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException(string.Format("Directory was not found: {0}", directory));

		    return GetPackedFolder(new DirectoryInfo(directory), zipContentOnly);
		}

        /// <summary>
        /// Gets the zipped bytes.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="zipContentOnly">if set to <c>true</c> places the content into the root folder, otherwise </param>
        /// <returns></returns>
		public byte[] GetPackedFolder(DirectoryInfo directory, bool zipContentOnly = false)
		{
			if (directory == null)
				return null;

			using (var zipFile = new ZipFile())
			{
				zipFile.AddDirectory(directory.FullName, zipContentOnly ? string.Empty : directory.Name);
				using (var stream = new MemoryStream())
				{
					zipFile.Save(stream);
					return stream.ToArray();
				}
			}
		}

	    /// <summary>
	    /// Unpacks the folder.
	    /// </summary>
	    /// <param name="updateZipBytes"></param>
	    /// <param name="folderPath">The folder path.</param>
	    public void UnpackFolder(byte[] updateZipBytes, string folderPath)
	    {
            using (var zipStream = new MemoryStream(updateZipBytes))
            {
                var zipFile = ZipFile.Read(zipStream);
                zipFile.ExtractAll(folderPath, ExtractExistingFileAction.OverwriteSilently);
            }
	    }
	}
}
