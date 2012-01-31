using System;
using System.Collections.Generic;
using NDistribUnit.Common.Logging;

namespace NDistribUnit.Common.Common.Domains
{
    internal class InAnotherDomainResolverCreator : MarshalByRefObject, IDisposable
    {
        private AssemblyResolver resolver;

        public void CreateAssemblyResolver(IEnumerable<string> paths)
        {
            string appDataKey = typeof (AssemblyResolver).FullName;
            resolver = (AssemblyResolver)AppDomain.CurrentDomain.GetData(appDataKey) 
                       ?? new AssemblyResolver(new ConsoleLog());
            AppDomain.CurrentDomain.SetData(appDataKey, resolver);

            foreach (var path in paths)
            {
                resolver.AddDirectory(path);
            }
        }

        public void Dispose()
        {
            resolver.Dispose();
        }
    }
}