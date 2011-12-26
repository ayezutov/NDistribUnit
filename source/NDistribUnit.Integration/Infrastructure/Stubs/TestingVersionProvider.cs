using System;
using NDistribUnit.Common.Common.Updating;

namespace NDistribUnit.Integration.Tests.Infrastructure.Stubs
{
    public class TestingVersionProvider: IVersionProvider
    {
        private Version version;

        public TestingVersionProvider(Version version)
        {
            this.version = version;
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <returns></returns>
        public Version GetVersion()
        {
            return version;
        }

        public void SetVersion(Version newVersion)
        {
            version = newVersion;
        }
    }
}