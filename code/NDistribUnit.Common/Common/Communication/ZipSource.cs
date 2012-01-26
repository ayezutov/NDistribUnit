using System.IO;
using Ionic.Zip;
using Ionic.Zlib;

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
                ConfigureZip(zipFile);
                zipFile.AddDirectory(directory.FullName, zipContentOnly ? string.Empty : directory.Name);
			    var stream = fileStream ?? File.Create(Path.GetTempFileName(), 1024*1024,
			                             FileOptions.DeleteOnClose | FileOptions.SequentialScan);
				zipFile.Save(stream);
			    stream.Seek(0, SeekOrigin.Begin);
				return stream;
			}
		}

	    private static void ConfigureZip(ZipFile zipFile)
	    {
	        zipFile.ParallelDeflateThreshold = -1;
	        zipFile.CompressionLevel = CompressionLevel.BestCompression;
	        zipFile.CompressionMethod = CompressionMethod.Deflate;
	        zipFile.Encryption = EncryptionAlgorithm.None;
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
                    ConfigureZip(zipFile);
                    zipFile.ExtractAll(folderPath, ExtractExistingFileAction.OverwriteSilently);
                }
            }
	    }
	}
}
