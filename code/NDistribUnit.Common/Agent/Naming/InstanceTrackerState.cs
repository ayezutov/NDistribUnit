using System;

namespace NDistribUnit.Common.Agent.Naming
{
    /// <summary>
    /// The state of instance tracker
    /// </summary>
    [Serializable]
    public class InstanceTrackerState
    {
        /// <summary>
        /// Gets the number.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is initialized; otherwise, <c>false</c>.
        /// </value>
        public bool IsInitialized { get; set; }
    }
}