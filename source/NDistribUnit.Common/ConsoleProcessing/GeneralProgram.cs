using System;
using System.Threading;
using Autofac;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Common.ConsoleProcessing
{
	/// <summary>
	/// 
	/// </summary>
	public class GeneralProgram
	{
		/// 
		protected UpdatesAvailabilityMonitor updatesMonitor;

		/// <summary>
		/// Waits until console input or available update and get return code.
		/// </summary>
		/// <returns></returns>
		protected int WaitAndGetReturnCode()
		{
			var returnSemaphore = new Semaphore(0, 1);
			var returnCodeAccessMutex = new Mutex();
			int? returnCode = null;

			updatesMonitor.UpdateAvailable += () =>
			{
				returnCodeAccessMutex.WaitOne();

				if (returnCode == null)
				{
					returnCode = (int)ReturnCodes.RestartDueToAvailableUpdate;
					returnSemaphore.Release();
				}

				returnCodeAccessMutex.ReleaseMutex();
			};
			var thread = new Thread(() =>
			{
				string line;
				do
				{
					line = ReadLineNonBlocking();
				} while (line == null || !line.Equals("exit", StringComparison.OrdinalIgnoreCase));

				returnCodeAccessMutex.WaitOne();

				if (returnCode == null)
				{
					returnCode = 0;
					returnSemaphore.Release();
				}

				returnCodeAccessMutex.ReleaseMutex();
			});
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
				while (!System.Console.KeyAvailable)
					Thread.Sleep(100);

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

		/// <summary>
		/// Registers the common dependencies.
		/// </summary>
		protected static void RegisterCommonDependencies(ContainerBuilder builder)
		{
			builder.RegisterType<UpdatesAvailabilityMonitor>();
			builder.RegisterType<VersionDirectoryFinder>();
			builder.RegisterType<UpdateReceiver>();
			builder.Register(c => new RollingLog(1000)).InstancePerLifetimeScope();
			builder.Register(c => new CombinedLog(new ConsoleLog(), c.Resolve<RollingLog>())).As<ILog>().InstancePerLifetimeScope();
		}
	}

	
}