using System;
using System.Reflection;

namespace NDistribUnit.Common.Common.Updating
{
    class CurrentAssemblyVersionProvider : IVersionProvider
    {
        public Version GetVersion()
        {
            return Assembly.GetCallingAssembly().GetName().Version;
        }
    }
}