using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NDistribUnit.Common.HashChecks;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Bootstrapper
{
	internal class BootstrapperProgram
	{
		private static void Main(string[] args)
		{
			var log = new ConsoleLog();
			string assemblyFile = Assembly.GetEntryAssembly().Location;
			string assemblyFolder = Path.GetDirectoryName(assemblyFile);
			string assemblyFileName = Path.GetFileNameWithoutExtension(assemblyFile);
			
			var versionDirectory = new VersionDirectoryFinder(log).GetVersionDirectory(assemblyFolder);

			if (versionDirectory == null)
			{
				log.Error("Cannot find any valid directory, which has a name with version pattern.");
				Console.ReadKey();
				return;
			}

			var exeDirectory = new DirectoryInfo(Path.Combine(versionDirectory.FullName, assemblyFileName));
			var files = exeDirectory.GetFiles("*.exe");
			if (files.Length == 0)
				throw new FileNotFoundException(string.Format("Cannot find a file with 'exe' extension inside '{0}'",
				                                              exeDirectory.FullName));
			if (files.Length > 1)
				throw new InvalidOperationException(string.Format("There is more than 1 file with 'exe' extension inside '{0}'",
				                                                  exeDirectory.FullName));

			new Process
				{
					StartInfo = new ProcessStartInfo(files[0].FullName,
					                                 new BootstrapperParameters
					                                 	{
					                                 		BootstrapperFile = assemblyFile,
					                                 		ConfigurationFile = assemblyFile + ".config",
															IsDebug = Debugger.IsAttached
					                                 	}.ToString())
				}.Start();
		}
	}
}