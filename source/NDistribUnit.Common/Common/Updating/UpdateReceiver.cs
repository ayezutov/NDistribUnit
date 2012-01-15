using System.IO;
using System.Threading;
using Ionic.Zip;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.ServiceContracts;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Common.Common.Updating
{
	/// <summary>
	/// Receives an update package and applies it onto the currently running instance
	/// </summary>
	public class UpdateReceiver: IUpdateReceiver
	{
		private readonly VersionDirectoryFinder finder;
		private readonly BootstrapperParameters parameters;
	    private readonly ZipSource zip;

	    /// <summary>
        /// Initializes a new instance of the <see cref="UpdateReceiver"/> class.
        /// </summary>
        /// <param name="finder">The finder.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="zip">The zip.</param>
		public UpdateReceiver(VersionDirectoryFinder finder, BootstrapperParameters parameters, ZipSource zip)
		{
			this.finder = finder;
			this.parameters = parameters;
            this.zip = zip;
		}

	    /// <summary>
	    /// Applies the update.
	    /// </summary>
	    /// <param name="package">The package.</param>
	    public void SaveUpdatePackage(UpdatePackage package)
		 {
			 if (!package.IsAvailable)
			 	return;

			 var versionDirectory = finder.GetVersionDirectory(parameters.RootFolder, package.Version);
			 if (versionDirectory != null)
				 return;

			 var mutex = new Mutex(false, parameters.RootFolder.Replace("\\", "|"));
			 mutex.WaitOne();

			 try
			 {
				 versionDirectory = finder.GetVersionDirectory(parameters.RootFolder, package.Version);
				 if (versionDirectory != null)
					 return;

                 var targetDir = parameters.RootFolder;
                 if (!Directory.Exists(targetDir))
                     Directory.CreateDirectory(targetDir);

                 zip.UnpackFolder(package.UpdateZipStream, targetDir);
			 }
			 finally
			 {
				 mutex.ReleaseMutex();
			 }
		 }
	}
}