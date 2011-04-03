using System;

namespace NDistribUnit.Common.Communication.ConnectionTracking.Announcement
{
    /// <summary>
    /// Options for announcement-based connections tracker
    /// </summary>
    public class AnnouncementConnectionsTrackerOptions: IConnectionsTrackerOptions
    {
        /// <summary>
        /// Gets the ping interval in miliseconds.
        /// </summary>
        public int PingIntervalInMiliseconds { get; set; }

        /// <summary>
        /// Gets or sets the scope.
        /// </summary>
        /// <value>
        /// The scope.
        /// </value>
        public Uri Scope { get; set; }
    }
}