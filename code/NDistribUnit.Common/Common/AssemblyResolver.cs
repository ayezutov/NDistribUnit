using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using NDistribUnit.Common.Logging;

namespace NDistribUnit.Common.Common
{
    /// <summary>
    /// Resolves assemblies
    /// </summary>
    public class AssemblyResolver : MarshalByRefObject, IDisposable
    {
        private readonly ILog log;
        private readonly Dictionary<string, Assembly> cache = new Dictionary<string, Assembly>();
        private readonly ArrayList directories = new ArrayList();
        private AppDomain domain;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyResolver"/> class.
        /// </summary>
        public AssemblyResolver(ILog log): this(log, AppDomain.CurrentDomain)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyResolver"/> class.
        /// </summary>
        public AssemblyResolver(ILog log, AppDomain domain)
        {
            this.log = log;
            this.domain = domain;
            this.domain.AssemblyResolve += DomainAssemblyResolve;
        }



        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            domain.AssemblyResolve -= DomainAssemblyResolve;
        }

        /// <summary>
        /// Adds a single file to resolver.
        /// </summary>
        /// <param name="file">The file.</param>
        public void AddFile(string file)
        {
            Assembly assembly;
            
            try
            {
                assembly = Assembly.LoadFrom(file);
            }
            catch(Exception ex)
            {
                if (log != null)
                    log.Warning("Error while adding assembly", ex);
                return;
            }

            cache[assembly.GetName().FullName] = assembly;
        }

        /// <summary>
        /// Adds several files from a directory by pattern.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="pattern">The pattern.</param>
        public void AddFiles(string directory, string pattern)
        {
            if (Directory.Exists(directory))
                foreach (string file in Directory.GetFiles(directory, pattern))
                    AddFile(file);
        }

        /// <summary>
        /// Adds the directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        public void AddDirectory(string directory)
        {
            if (Directory.Exists(directory))
                directories.Add(directory);
        }

        private Assembly DomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string fullName = args.Name;
            var assemblyName = new AssemblyName(fullName);
            
            if (cache.ContainsKey(fullName))
                return cache[fullName];

            foreach (string dir in directories)
            {
                foreach (string file in Directory.GetFiles(dir, "*.dll", SearchOption.AllDirectories))
                {
                    AssemblyName pretendentFullName;
                    try
                    {
                        pretendentFullName = AssemblyName.GetAssemblyName(file);
                    }
                    catch(Exception ex)
                    {
                        if (log != null)
                            log.Warning("Assembly check failed", ex);
                        continue;
                    }

                    if (pretendentFullName.FullName == fullName
                        || (pretendentFullName.Name == assemblyName.Name 
                            && AreEqual(pretendentFullName.GetPublicKeyToken(), assemblyName.GetPublicKeyToken()) 
                            && AreEqual(pretendentFullName.CultureInfo, pretendentFullName.CultureInfo)))
                    {
                        if (log != null)
                            log.Info(string.Format("Added to Cache: {0}", file));
                        cache[fullName] = Assembly.LoadFrom(file);
                        return cache[fullName];
                    }
                }
            }

            if (log != null)
                log.Debug(string.Format("Not in Cache: {0}", fullName));
            return null;
        }

        private bool AreEqual(CultureInfo cultureInfo, CultureInfo cultureInfo2)
        {
            if (cultureInfo == null && cultureInfo2 == null)
                return true;

            if (cultureInfo == null || cultureInfo2 == null)
                return false;

            return cultureInfo.Equals(cultureInfo2);
        }

        private bool AreEqual(byte[] bytes1, byte[] bytes2)
        {
            if (bytes1 == null && bytes2 == null)
                return true;

            if (bytes1 == null || bytes2 == null)
                return false;

            if (bytes1.Length != bytes2.Length)
                return false;

            return !bytes1.Where((t, i) => t != bytes2[i]).Any();
        }
    }
}