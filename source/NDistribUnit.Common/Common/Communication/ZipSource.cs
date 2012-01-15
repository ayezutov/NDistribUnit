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
	    /// <param name="zipContentOnly">if set to <c>true</c> [zip content only].</param>
	    /// <param name="fileStream"> </param>
	    /// <returns></returns>
	    public Stream GetPackedFolder(DirectoryInfo directory, bool zipContentOnly = false, FileStream fileStream = null)
		{

			if (directory == null)
				return null;

            if (!directory.Exists)
                throw new DirectoryNotFoundException(string.Format("Directory was not found: {0}", directory));

            using (var zipFile = new ZipFile())
			{
				zipFile.AddDirectory(directory.FullName, zipContentOnly ? string.Empty : directory.Name);
			    var stream = fileStream ?? File.Create(Path.GetTempFileName(), 1024*1024,
			                             FileOptions.DeleteOnClose | FileOptions.SequentialScan);
				zipFile.Save(stream);
			    stream.Seek(0, SeekOrigin.Begin);
				return stream;
			}
		}

        /// <summary>
        /// Unpacks the folder.
        /// </summary>
        /// <param name="zipStream">The zip stream.</param>
        /// <param name="folderPath">The folder path.</param>
        public void UnpackFolder(Stream zipStream, string folderPath)
	    {
            using (zipStream)
            {
                using (var zipFile = ZipFile.Read(zipStream))
                {
                    zipFile.ExtractAll(folderPath, ExtractExistingFileAction.OverwriteSilently);
                }
            }
	    }
	}
}
