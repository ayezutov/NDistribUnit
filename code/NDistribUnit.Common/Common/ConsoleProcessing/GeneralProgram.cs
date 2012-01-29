using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Updating;
using NUnit.Util;
using NDistribUnit.Common.Common.Extensions;

namespace NDistribUnit.Common.Common.ConsoleProcessing
{
    /// <summary>
    /// 
    /// </summary>
    public class GeneralProgram
    {
        // field to prevent garbage collection
        private readonly AssemblyResolver resolver;

        /// 
        protected readonly UpdatesMonitor updatesMonitor;

        ///
        protected readonly ExceptionCatcher exceptionCatcher;

        ///
        protected readonly ILog log;

        ///
        protected readonly BootstrapperParameters bootstrapperParameters;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralProgram"/> class.
        /// </summary>
        /// <param name="resolver">The resolver.</param>
        /// <param name="updatesMonitor">The updates monitor.</param>
        /// <param name="exceptionCatcher">The exception catcher.</param>
        /// <param name="log">The log.</param>
        /// <param name="bootstrapperParameters"> </param>
        protected GeneralProgram(AssemblyResolver resolver, UpdatesMonitor updatesMonitor, ExceptionCatcher exceptionCatcher, ILog log, BootstrapperParameters bootstrapperParameters)
        {
            this.resolver = resolver;
            this.updatesMonitor = updatesMonitor;
            this.exceptionCatcher = exceptionCatcher;
            this.log = log;
            this.bootstrapperParameters = bootstrapperParameters;
            resolver.AddDirectory(GetNUnitFolder(bootstrapperParameters));
        }

        /// <summary>
        /// Gets the N unit folder.
        /// </summary>
        /// <param name="bootstrapperParameters">The bootstrapper parameters.</param>
        /// <returns></returns>
        public static string GetNUnitFolder(BootstrapperParameters bootstrapperParameters)
        {
            string versionFolder = 
                !string.IsNullOrEmpty(bootstrapperParameters.RootFolder)
                ? GetFolderWithMaximumVersionPattern(Path.Combine(bootstrapperParameters.RootFolder, "NUnit"))
                : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string temp = Path.Combine(versionFolder, "net-2.0");
            if (Directory.Exists(temp))
                return temp;
            
            return versionFolder;
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


        /// <summary>
        /// Waits until console input or available update and get return code.
        /// </summary>
        /// <param name="restartOnTheseFilesChange"></param>
        /// <returns></returns>
        protected int WaitAndGetReturnCode(IEnumerable<string> restartOnTheseFilesChange)
        {
            var returnSemaphore = new Semaphore(0, 1);
            var returnCodeAccessMutex = new Mutex();
            int? returnCode = null;

            foreach (var fileName in restartOnTheseFilesChange)
            {
                var fsw = new FileSystemWatcher(Path.GetDirectoryName(fileName), Path.GetFileName(fileName));
                fsw.Changed += (sender, args) =>
                {
                    exceptionCatcher.Run(() =>
                    {
                        returnCodeAccessMutex.WaitOne();

                        if (returnCode == null)
                        {
                            returnCode = (int)ReturnCodes.RestartDueToConfigChange;
                            returnSemaphore.Release();
                        }

                        returnCodeAccessMutex.ReleaseMutex();
                    });
                };
                fsw.EnableRaisingEvents = true;
            }

            updatesMonitor.UpdateAvailable += () =>
            {
                exceptionCatcher.Run(() =>
                {
                    returnCodeAccessMutex.WaitOne();

                    if (returnCode == null)
                    {
                        returnCode = (int)ReturnCodes.RestartDueToAvailableUpdate;
                        returnSemaphore.Release();
                    }

                    returnCodeAccessMutex.ReleaseMutex();
                });
            };

            var thread = new Thread(() => exceptionCatcher.Run(
                () =>
                {
                    string line;
                    do
                    {
                        line = ReadLineNonBlocking();
                        if (Thread.CurrentThread.ThreadState.IsOneOf(ThreadState.AbortRequested, ThreadState.Aborted))
                            return;
                    } while (line == null || !line.Equals("exit", StringComparison.OrdinalIgnoreCase));

                    returnCodeAccessMutex.WaitOne();

                    if (returnCode == null)
                    {
                        returnCode = 0;
                        returnSemaphore.Release();
                    }

                    returnCodeAccessMutex.ReleaseMutex();
                }));
            thread.Start();

            returnSemaphore.WaitOne();
            thread.Abort();
            return returnCode.GetValueOrDefault();
        }

        /// <summary>
        /// Reads the line non blocking.
        /// </summary>
        /// <returns></returns>
        private static string ReadLineNonBlocking()
        {
            string line = String.Empty;
            do
            {
                while (!System.Console.KeyAvailable 
                    && !Thread.CurrentThread.ThreadState.IsOneOf(ThreadState.AbortRequested, ThreadState.Aborted))
                    Thread.Sleep(100);

                if (Thread.CurrentThread.ThreadState.IsOneOf(ThreadState.AbortRequested, ThreadState.Aborted))
                    return null;

                ConsoleKeyInfo key = System.Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter)
                {
                    System.Console.CursorTop++;
                    System.Console.CursorLeft = 0;
                    break;
                }

                switch (key.Key)
                {
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.PageDown:
                    case ConsoleKey.PageUp:
                    case ConsoleKey.Home:
                    case ConsoleKey.End:
                        break;

                    case ConsoleKey.Backspace:
                        line = line.Substring(0, line.Length - 1);
                        System.Console.Write(key.KeyChar);
                        System.Console.Write(" ");
                        System.Console.Write(key.KeyChar);
                        break;

                    default:
                        line += key.KeyChar;
                        System.Console.Write(key.KeyChar);
                        break;
                }

            } while (true);

            return line;
        }
    }
    

}