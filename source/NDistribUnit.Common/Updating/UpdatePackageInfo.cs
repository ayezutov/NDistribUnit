using System;

namespace NDistribUnit.Common.Updating
{
    /// <summary>
    /// Gets the information of an available update package
    /// </summary>
    public class UpdatePackageInfo
    {
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public Version Version { get; set; }

        /// <summary>
        /// Gets or sets the update URI.
        /// </summary>
        /// <value>
        /// The update URI.
        /// </value>
        public Uri UpdateUri { get; set; }
    }
}