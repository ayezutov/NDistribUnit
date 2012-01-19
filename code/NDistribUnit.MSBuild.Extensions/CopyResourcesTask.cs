using System;
using System.ComponentModel;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Vestris.ResourceLib;
using System.Linq;

namespace NDistribUnit.MSBuild.Extensions
{
	/// <summary>
	/// 
	/// </summary>
	public class CopyResourcesTask : AppDomainIsolatedTask
	{
		/// <summary>
		/// Gets or sets the source files.
		/// </summary>
		/// <value>
		/// The source files.
		/// </value>
		public ITaskItem[] SourceFiles { get; set; }

		/// <summary>
		/// Gets or sets the target files.
		/// </summary>
		/// <value>
		/// The target files.
		/// </value>
		public ITaskItem[] TargetFiles { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [continue on error].
		/// </summary>
		/// <value>
		///   <c>true</c> if [continue on error]; otherwise, <c>false</c>.
		/// </value>
		public bool ContinueOnError { get; set; }

		public override bool Execute()
		{
			if (SourceFiles.Length != TargetFiles.Length)
			{
				Log.LogError("The number of source and target files differs.");
				return false;
			}

			for (int i = 0; i < SourceFiles.Length; i++)
			{
				string targetFile = TargetFiles[i].ItemSpec;
				string sourceFile = SourceFiles[i].ItemSpec;

				Log.LogMessage("Source file: {0}", sourceFile);
				Log.LogMessage("Target file: {0}", targetFile);
				
				if (!CopyManifestAndIcons(targetFile, sourceFile)) return false;
				if (!CopyFileProperties(targetFile, sourceFile)) return false;
			}

			return true;
		}

	    private bool CopyManifestAndIcons(string targetFile, string sourceFile)
	    {
	        using (var vi = new ResourceInfo())
	        {
	            try
	            {
	                vi.Load(sourceFile);
	            }
	            catch (Win32Exception)
	            {
	                if (ContinueOnError)
	                    return true;
	                throw;
	            }
	            foreach (ResourceId type in vi.ResourceTypes)
	            {
	                foreach (Resource resource in vi.Resources[type])
	                {
	                    if (resource is ManifestResource || resource is IconDirectoryResource)
	                    {
	                        try
	                        {
	                            resource.SaveTo(targetFile);
	                        }
	                        catch (Win32Exception)
	                        {
	                            if (ContinueOnError)
	                                continue;
	                            throw;
	                        }
	                    }
	                }
	            }
	        }
	        return true;
	    }
        
        private bool CopyFileProperties(string targetFile, string sourceFile)
	    {
            VersionResource targetVersion;

            using (var sourceInfo = new ResourceInfo())
            {
                using (var targetInfo = new ResourceInfo())
                {
                    try
                    {
                        sourceInfo.Load(sourceFile);
                        targetInfo.Load(targetFile);
                    }
                    catch (Win32Exception)
                    {
                        if (ContinueOnError)
                            return true;
                        throw;
                    }

                    VersionResource sourceVersion = sourceInfo.OfType<VersionResource>().FirstOrDefault();

                    targetVersion = targetInfo.OfType<VersionResource>().FirstOrDefault();

                    var valuesToCopy = new[] { "FileDescription", "InternalName" };

                    StringTable sourceDefaultStringTable = ((StringFileInfo)(sourceVersion["StringFileInfo"])).Default;
                    StringTable targetDefaultStringTable = ((StringFileInfo)(targetVersion["StringFileInfo"])).Default;

                    foreach (var value in valuesToCopy)
                    {
                        targetDefaultStringTable.Strings[value].Value = sourceDefaultStringTable.Strings[value].Value;
                    }
                }
            }

            try
            {
                targetVersion.SaveTo(targetFile);
            }
            catch (Win32Exception)
            {
                if (ContinueOnError)
                    return true;
                throw;
            }
            

            return true;
	    }
	}
}
