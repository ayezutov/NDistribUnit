using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Updating;
using NDistribUnit.Common.Common.Extensions;

namespace NDistribUnit.Common.Common.ConsoleProcessing
{
    /// <summary>
    /// 
    /// </summary>
    public class GeneralProgram
    {
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
        /// <param name="updatesMonitor">The updates monitor.</param>
        /// <param name="exceptionCatcher">The exception catcher.</param>
        /// <param name="log">The log.</param>
        /// <param name="bootstrapperParameters"> </param>
        protected GeneralProgram(UpdatesMonitor updatesMonitor, ExceptionCatcher exceptionCatcher, ILog log, BootstrapperParameters bootstrapperParameters)
        {
            this.updatesMonitor = updatesMonitor;
            this.exceptionCatcher = exceptionCatcher;
            this.log = log;
            this.bootstrapperParameters = bootstrapperParameters;
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
                        try
                        {
                            line = ReadLineNonBlocking();
                        }
                        catch(ThreadAbortException)
                        {
                            return;
                        }
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