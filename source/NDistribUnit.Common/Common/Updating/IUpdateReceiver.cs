using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Common.Common.Updating
{
    /// <summary>
    /// 
    /// </summary>
    public interface IUpdateReceiver
    {
        /// <summary>
        /// Applies the update.
        /// </summary>
        /// <param name="package">The package.</param>
        bool SaveUpdatePackage(UpdatePackage package);
    }
}