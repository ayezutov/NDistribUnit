using System;

namespace NDistribUnit.Common.Server.AgentsTracking
{
    /// <summary>
    /// The options of a connection tracker
    /// </summary>
    public interface IConnectionsHostOptions
    {
        /// <summary>
        /// Gets the ping interval in miliseconds.
        /// </summary>
        int PingIntervalInMiliseconds { get; }

        /// <summary>
        /// Gets or sets the scope.
        /// </summary>
        /// <value>
        /// The scope.
        /// </value>
        Uri Scope { get; set; }
    }
}