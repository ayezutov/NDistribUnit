using System;
using System.Diagnostics;
using NDistribUnit.Common.Agent;
using NDistribUnit.Common.Updating.Updaters;

namespace NDistribUnit.Common.Updating
{
	/// <summary>
	/// 
	/// </summary>
	public class AgentUpdater: UpdaterBase
	{
		private readonly AgentHost host;
		private readonly BootstrapperParameters bootstrapperParameters;

		/// <summary>
		/// Initializes a new instance of the <see cref="AgentUpdater"/> class.
		/// </summary>
		/// <param name="host">The host.</param>
		/// <param name="bootstrapperParameters">The bootstrapper parameters.</param>
		public AgentUpdater(AgentHost host, BootstrapperParameters bootstrapperParameters)
		{
			this.host = host;
			this.bootstrapperParameters = bootstrapperParameters;
		}

		/// <summary>
		/// Performs the update.
		/// </summary>
		public override void PerformUpdate()
		{
			//TODO: Save agent's state here
			//host.SaveState();
			host.Stop();

			Restart(bootstrapperParameters);
		}
	}
}