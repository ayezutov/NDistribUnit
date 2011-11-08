using System.Collections.Generic;

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
        byte[] GetPackage(IList<string> projectFiles);
    }
}