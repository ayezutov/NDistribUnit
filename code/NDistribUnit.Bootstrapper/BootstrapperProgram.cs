using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Bootstrapper
{
    public class UnhandledExceptionLogger: MarshalByRefObject
    {
        private ILog log;
        
        public UnhandledExceptionLogger()
        {
            log = new CombinedLog(new ConsoleLog(), new WindowsLog("Bootstrapper"));
        }

        public void ProcessUnhandledExceptions()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                                                              {
                                                                  log.Error("Unhandled exception was handled by bootstrapper", (Exception)args.ExceptionObject);  
                                                              };
        }
    }

    [Serializable]
	internal class BootstrapperProgram
	{
		private readonly string[] args;
		private ILog log;
		private AppDomain domain;

        [STAThread]
		private static void Main(string[] args)
		{
			new BootstrapperProgram(args).Run();
		}

		private BootstrapperProgram(string[] args)
		{
			this.args = args;
            log = new CombinedLog(new ConsoleLog(), new WindowsLog("Bootstrapper"));
		}

		private void Run()
		{
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

            var targetFile = files[0].FullName;
            var targetFolder = Path.GetDirectoryName(targetFile);

		    var targetFileConfig = GetConfigurationFilename(targetFile);

		    var currentFileConfig = GetConfigurationFilename(assemblyFile);

            //targetFileConfig = new ConfigurationFileMerger().MergeFiles(targetFileConfig, currentFileConfig);
            
		    try
			{
				domain = AppDomain.CreateDomain(AppDomain.CurrentDomain.FriendlyName + "_bootstrapped",
				                                    AppDomain.CurrentDomain.Evidence,
				                                    new AppDomainSetup
				                                    	{
				                                    		ConfigurationFile = targetFileConfig,
                                                            ApplicationBase = targetFolder,
				                                    	});

			    var logger = (UnhandledExceptionLogger)domain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location,
                                                   typeof(UnhandledExceptionLogger).FullName,
			                                       false, BindingFlags.Default, null, null,
			                                       Thread.CurrentThread.CurrentCulture, null);
			    logger.ProcessUnhandledExceptions();

			    BootstrapperParameters.WriteToDomain(new BootstrapperParameters
			        {
			            BootstrapperFile = assemblyFile,
			            ConfigurationFile = currentFileConfig,
			        }, domain);
                

//				var newArgs = new List<string>(args);
//				newArgs.AddRange(.ToArray());
				var returnValue = domain.ExecuteAssembly(targetFile, args);
				AppDomain.Unload(domain);
			    if (returnValue == (int) ReturnCodes.RestartDueToAvailableUpdate || returnValue == (int) ReturnCodes.RestartDueToConfigChange)
			        Main(args);
			}
			catch (Exception ex)
			{
				log.Error("An error occurred while running bootstrapped program", ex);
				Console.ReadLine();
			}
		}
        
	    private string GetConfigurationFilename(string exeName)
	    {
	        string possibleName = exeName + ".config";
            if (File.Exists(possibleName))
                return possibleName;

	        possibleName = Path.ChangeExtension(exeName, ".config");
            if (File.Exists(possibleName))
                return possibleName;

	        return null;
	    }
	}
}