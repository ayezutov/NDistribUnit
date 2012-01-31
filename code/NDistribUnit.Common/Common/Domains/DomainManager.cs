using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Common.Common.Domains
{
    /// <summary>
    /// Manages the creation
    /// </summary>
    public class DomainManager
    {
        /// <summary>
        /// Creates the domain.
        /// </summary>
        /// <param name="friendlyName">Name of the friendly.</param>
        /// <param name="basePath"> </param>
        /// <param name="configurationFile"> </param>
        /// <param name="pathsToResolve">The paths to resolve.</param>
        /// <returns></returns>
        public static AppDomain CreateDomain(string friendlyName, string basePath, string configurationFile, string[] pathsToResolve = null)
        {
            var appDomainSetup = new AppDomainSetup
                                     {
                                         ApplicationBase = basePath,
                                         ConfigurationFile = !String.IsNullOrEmpty(configurationFile)
                                                                 ? configurationFile
                                                                 : AppDomain.CurrentDomain.SetupInformation.
                                                                       ConfigurationFile
                                     };

            AppDomain appDomain = AppDomain.CreateDomain(friendlyName, null, appDomainSetup);

            if (pathsToResolve != null && pathsToResolve.Length > 0)
            {
                AddResolverForPaths(appDomain, pathsToResolve);
            }

            return appDomain;
        }

        /// <summary>
        /// Adds the resolver for paths.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="pathsToResolve">The paths to resolve.</param>
        public static void AddResolverForPaths(AppDomain domain, IEnumerable<string> pathsToResolve)
        {
            var testsResolverCreatorType = typeof(InAnotherDomainResolverCreator);

            var testsResolverCreator = (InAnotherDomainResolverCreator)Activator.CreateInstance(domain, testsResolverCreatorType.Assembly.FullName,
                                                                testsResolverCreatorType.FullName).Unwrap();
            testsResolverCreator.CreateAssemblyResolver(pathsToResolve);
        }

        /// <summary>
        /// Unloads the domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        public static void UnloadDomain(AppDomain domain)
        {
            try
            {
                AppDomain.Unload(domain);
            }
            catch(CannotUnloadAppDomainException)
            {}
        }

        /// <summary>
        /// Gets the N unit folder.
        /// </summary>
        /// <param name="bootstrapperParameters">The bootstrapper parameters.</param>
        /// <returns></returns>
        public static string[] GetNUnitFolders(BootstrapperParameters bootstrapperParameters)
        {
            string versionFolder = 
                !String.IsNullOrEmpty(bootstrapperParameters.RootFolder)
                    ? GetFolderWithMaximumVersionPattern(Path.Combine(bootstrapperParameters.RootFolder, "NUnit"))
                    : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (versionFolder == null)
                return new string[0];

            string temp = Path.Combine(versionFolder, "net-2.0");
            if (Directory.Exists(temp))
                return new[]{temp};
            
            return new[]{versionFolder};
        }

        private static string GetFolderWithMaximumVersionPattern(string rootFolder)
        {
            Tuple<Version, string> result = null;
            foreach (var directory in new DirectoryInfo(rootFolder).GetDirectories())
            {
                if (UpdatesMonitor.VersionPattern.IsMatch(directory.FullName))
                {
                    var current = new Tuple<Version, string>(Version.Parse(directory.Name), directory.FullName);
                    if (result == null)
                    {
                        result = current;
                    }
                    else if (result.Item1 < current.Item1)
                    {
                        result = current;
                    }
                }
            }

            return result == null ? null : result.Item2;
        }
    }
}