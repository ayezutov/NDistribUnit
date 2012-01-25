namespace NDistribUnit.Common.Communication
{
	/// <summary>
	/// 
	/// </summary>
	public enum ReturnCodes
	{
		/// 
		Success = 0,

		///
		UnhandledException = int.MaxValue,

		///
		RestartDueToAvailableUpdate = int.MaxValue - 1,

		///
		CannotLaunchBootstrappedApplicationDirectly = int.MaxValue - 2,
		
		///
		IncompleteParameterList = int.MaxValue - 3,

        ///
        RestartDueToConfigChange = int.MaxValue - 4,
	    
        ///
        ServerNotAvailable = int.MaxValue - 5,

        ///
        NetworkConnectivityError = int.MaxValue - 6,
        
        ///
        NoTestsAvailable = int.MaxValue - 7,
	}
}