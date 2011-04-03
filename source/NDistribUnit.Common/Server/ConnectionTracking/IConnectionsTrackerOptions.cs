namespace NDistribUnit.Common.Communication.ConnectionTracking
{
    /// <summary>
    /// The options of a connection tracker
    /// </summary>
    public interface IConnectionsTrackerOptions
    {
        /// <summary>
        /// Gets the ping interval in miliseconds.
        /// </summary>
        int PingIntervalInMiliseconds { get; }
    }
}