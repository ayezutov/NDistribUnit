using NDistribUnit.Common.Server.Communication;
using NDistribUnit.Common.Updating.Updaters;

namespace NDistribUnit.Common.Updating
{
	/// <summary>
	/// 
	/// </summary>
	public class ServerUpdater : UpdaterBase
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
		public override void PerformUpdate()
		{
			//TODO: Save server's state here
			//serverHost.SaveState();
			serverHost.Close();

			Restart(bootstrapperParameters);
		}
	}
}