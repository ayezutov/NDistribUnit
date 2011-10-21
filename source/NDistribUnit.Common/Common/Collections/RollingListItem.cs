using System.Diagnostics;

namespace NDistribUnit.Common.Collections
{
    /// <summary>
    /// A single item in rolling list
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    [DebuggerDisplay("{Value}")]
    public class RollingListItem<TValue>
    {
        /// <summary>
        /// Gets or sets the value of the item
        /// </summary>
        public TValue Value { get; set; }

        /// <summary>
        /// Gets or sets the previous item in the list
        /// </summary>
        public RollingListItem<TValue> Previous { get; internal set; }

        /// <summary>
        /// Gets or sets the next item in the list
        /// </summary>
        public RollingListItem<TValue> Next { get; set; }
    }
}