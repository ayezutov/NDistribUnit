namespace NDistribUnit.Common.Updating.Updaters
{
	/// <summary>
	/// Performs the updates on client side
	/// </summary>
	public class ClientUpdater : UpdaterBase
	{
		private readonly BootstrapperParameters bootstrapperParameters;

		/// <summary>
		/// Initializes a new instance of the <see cref="ClientUpdater"/> class.
		/// </summary>
		/// <param name="bootstrapperParameters">The bootstrapper parameters.</param>
		public ClientUpdater(BootstrapperParameters bootstrapperParameters)
		{
			this.bootstrapperParameters = bootstrapperParameters;
		}

		/// <summary>
		/// Performs the update.
		/// </summary>
		public override void PerformUpdate()
		{
			Restart(bootstrapperParameters);
		}
	}
}