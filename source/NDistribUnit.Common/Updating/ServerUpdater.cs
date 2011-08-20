using System;
using System.Diagnostics;
using NDistribUnit.Common.Server.Communication;

namespace NDistribUnit.Common.Updating
{
	/// <summary>
	/// 
	/// </summary>
	public class ServerUpdater : IUpdater
	{
		private readonly ServerHost serverHost;
		private readonly BootstrapperParameters bootstrapperParameters;

		/// <summary>
		/// Initializes a new instance of the <see cref="ServerUpdater"/> class.
		/// </summary>
		/// <param name="serverHost">The server host.</param>
		/// <param name="bootstrapperParameters">The bootstrapper parameters.</param>
		public ServerUpdater(ServerHost serverHost, BootstrapperParameters bootstrapperParameters)
		{
			this.serverHost = serverHost;
			this.bootstrapperParameters = bootstrapperParameters;
		}

		/// <summary>
		/// Performs the update.
		/// </summary>
		public void PerformUpdate()
		{
			var bootstrapperProcess = new Process
				{
					StartInfo = new ProcessStartInfo
					            	{
					            		FileName = bootstrapperParameters.BootstrapperFile,
					            		Arguments = "/restart"
					            	}
				};
			//TODO: Save server's state here
			//serverHost.SaveState();
			serverHost.Close();
			bootstrapperProcess.Start();
			Environment.Exit(0);
		}
	}
}