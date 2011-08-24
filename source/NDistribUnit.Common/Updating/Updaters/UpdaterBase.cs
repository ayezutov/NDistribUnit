using System;
using System.Diagnostics;

namespace NDistribUnit.Common.Updating.Updaters
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class UpdaterBase: IUpdater
	{
		/// <summary>
		/// Performs the update.
		/// </summary>
		public abstract void PerformUpdate();

		/// <summary>
		/// Restarts the specified parameters.
		/// </summary>
		/// <param name="parameters">The parameters.</param>
		protected void Restart(BootstrapperParameters parameters)
		{
			var bootstrapperProcess = new Process
			                          	{
			                          		StartInfo = new ProcessStartInfo
			                          		            	{
			                          		            		FileName = parameters.BootstrapperFile,
			                          		            		Arguments = "/restart"
			                          		            	}
			                          	};

			bootstrapperProcess.Start();
			Environment.Exit(0);
		}
	}
}