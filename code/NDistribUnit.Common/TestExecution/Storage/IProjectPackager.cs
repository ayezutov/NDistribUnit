using System.Collections.Generic;
using System.IO;

namespace NDistribUnit.Common.TestExecution.Storage
{
    /// <summary>
    /// Packages the project
    /// </summary>
    public interface IProjectPackager
    {
        /// <summary>
        /// Gets the package.
        /// </summary>
        /// <param name="projectFiles">The project files.</param>
        /// <returns></returns>
        Stream GetPackage(IList<string> projectFiles);
    }
}