namespace NDistribUnit.Common.Updating
{
    /// <summary>
    /// Gives access to available updates' information
    /// </summary>
    public interface IUpdateSource
    {
        /// <summary>
        /// Gets the available updates.
        /// </summary>
        /// <returns></returns>
        UpdatePackageInfo[] GetAvailableUpdates();
    }
}