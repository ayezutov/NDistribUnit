using System;
using System.Collections.Generic;
using System.IO;

namespace NDistribUnit.Common.Updating
{
    /// <summary>
    /// Get the updates from the folder, which holds updates
    /// </summary>
    public class FromUpdateFolderUpdateSource : IUpdateSource
    {
        private readonly string baseFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="FromUpdateFolderUpdateSource"/> class.
        /// </summary>
        /// <param name="baseFolder">The base folder.</param>
        public FromUpdateFolderUpdateSource(string baseFolder)
        {
            this.baseFolder = baseFolder;
        }

        /// <summary>
        /// Gets the available updates.
        /// </summary>
        /// <returns></returns>
        public UpdatePackageInfo[] GetAvailableUpdates()
        {
            var result = new List<UpdatePackageInfo>();
            var directory = new DirectoryInfo(baseFolder);

            foreach (var subDirectory in directory.GetDirectories())
            {
                Version version;
                if (Version.TryParse(subDirectory.Name, out version))
                {
                    result.Add(new UpdatePackageInfo{Version = version, UpdateUri = new Uri(subDirectory.FullName)});
                }
            }
            return result.ToArray();
        }
    }
}