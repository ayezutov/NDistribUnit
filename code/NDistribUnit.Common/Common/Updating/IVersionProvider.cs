using System;

namespace NDistribUnit.Common.Common.Updating
{
    /// <summary>
    /// 
    /// </summary>
    public interface IVersionProvider
    {
        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <returns></returns>
        Version GetVersion();
    }
}