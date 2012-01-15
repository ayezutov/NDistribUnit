using NDistribUnit.Common.Contracts.DataContracts;

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
        void SaveUpdatePackage(UpdatePackage package);
    }
}