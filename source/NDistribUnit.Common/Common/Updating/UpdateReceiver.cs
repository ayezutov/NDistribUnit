using System.IO;
using System.Threading;
using Ionic.Zip;
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

		/// <summary>
		/// Initializes a new instance of the <see cref="UpdateReceiver"/> class.
		/// </summary>
		/// <param name="finder">The finder.</param>
		/// <param name="parameters">The parameters.</param>
		public UpdateReceiver(VersionDirectoryFinder finder, BootstrapperParameters parameters)
		{
			this.finder = finder;
			this.parameters = parameters;
		}

		/// <summary>
		/// Applies the update.
		/// </summary>
		/// <param name="package">The package.</param>
		 public bool SaveUpdatePackage(UpdatePackage package)
		 {
			 if (!package.IsAvailable)
			 	return false;

			 var versionDirectory = finder.GetVersionDirectory(parameters.RootFolder, package.Version);
			 if (versionDirectory != null)
				 return false;

			 var mutex = new Mutex(false, parameters.RootFolder.Replace("\\", "|"));
			 mutex.WaitOne();

			 try
			 {
				 versionDirectory = finder.GetVersionDirectory(parameters.RootFolder, package.Version);
				 if (versionDirectory != null)
					 return false;

				 using (var zipStream = new MemoryStream(package.UpdateZipBytes))
				 {
					 var zipFile = ZipFile.Read(zipStream);
					 var targetDir = parameters.RootFolder;
					 if (!Directory.Exists(targetDir))
						 Directory.CreateDirectory(targetDir);
					 zipFile.ExtractAll(targetDir, ExtractExistingFileAction.OverwriteSilently);
				 }

			 	return true;
			 }
			 finally
			 {
				 mutex.ReleaseMutex();
			 }
		 }
	}
}