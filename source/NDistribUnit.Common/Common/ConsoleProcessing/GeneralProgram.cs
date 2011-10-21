using System;
using System.Threading;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Communication;

namespace NDistribUnit.Common.Common.ConsoleProcessing
{
	/// <summary>
	/// 
	/// </summary>
	public class GeneralProgram
	{
		/// 
		protected UpdatesMonitor updatesMonitor;

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
	}

    
}