using System;
using System.Threading;

namespace NDistribUnit.Common.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class Unlocker: IDisposable
    {
        private readonly Action disposeAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="Unlocker"/> class.
        /// </summary>
        /// <param name="disposeAction">The sync object.</param>
        public Unlocker(Action disposeAction)
        {
            this.disposeAction = disposeAction;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            disposeAction();
        }
    }
}