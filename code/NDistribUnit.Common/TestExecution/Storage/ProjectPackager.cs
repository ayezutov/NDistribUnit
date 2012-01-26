using System;
using System.Collections.Generic;
using System.IO;
using NDistribUnit.Common.Common.Communication;

namespace NDistribUnit.Common.TestExecution.Storage
{
    /// <summary>
    /// 
    /// </summary>
    public class ProjectPackager : IProjectPackager
    {
        private readonly ZipSource zip;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectPackager"/> class.
        /// </summary>
        /// <param name="zip">The source.</param>
        public ProjectPackager(ZipSource zip)
        {
            this.zip = zip;
        }

        /// <summary>
        /// Gets the package.
        /// </summary>
        /// <param name="projectFiles">The project files.</param>
        /// <returns></returns>
        public Stream GetPackage(IList<string> projectFiles)
        {
            if (projectFiles.Count == 0)
                throw new ArgumentException("There should be at least one project file", "projectFiles");

            if (projectFiles.Count > 1)
                throw new NotImplementedException();

            Stream packedFolder = zip.GetPackedFolder(new DirectoryInfo(Path.GetDirectoryName(projectFiles[0])), true);
            
            return packedFolder;
        }
    }
}