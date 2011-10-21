namespace NDistribUnit.Common
{
    /// <summary>
    /// An event arguments container for a variety of types
    /// </summary>
    /// <typeparam name="T">The type of the resulting data</typeparam>
    public class EventArgs<T>: System.EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventArgs&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public EventArgs(T data)
        {
            Data = data;
        }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public T Data { get; set; }
    }
}
