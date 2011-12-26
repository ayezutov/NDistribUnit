using System;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Retrying;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Integration.Tests.Infrastructure.Stubs
{
    /// <summary>
    /// 
    /// </summary>
    public class TestUpdateReceiver : IUpdateReceiver
    {
        private UpdatePackage updatePackage;

        public bool SaveUpdatePackage(UpdatePackage package)
        {
            this.updatePackage = package;
            return true;
        }

        public bool HasReceivedUpdate(Version version = null)
        {
            return Retry.UntilTrue(() => (version == null && updatePackage != null)
                                     ||
                                     (version != null && updatePackage != null &&
                                      updatePackage.Version.Equals(version)), 100);
        }
    }
}