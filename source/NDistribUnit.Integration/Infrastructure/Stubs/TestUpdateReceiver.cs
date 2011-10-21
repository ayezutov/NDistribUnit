using System;
using NDistribUnit.Common.Common.Updating;
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
            if (updatePackage == null)
                return false;

            if (version == null)
                return updatePackage != null;

            return version == updatePackage.Version;
        }
    }
}