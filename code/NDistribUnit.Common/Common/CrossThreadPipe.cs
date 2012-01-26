using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NDistribUnit.Common.Common
{
    /// <summary>
    /// The 
    /// </summary>
    public class CrossThreadPipe<TObject>
    {
        private IList<TObject> Buffer { get; set; }

        /// <summary>
        /// Gets the close timeout.
        /// </summary>
        public TimeSpan CloseTimeout { get; private set; }

        /// <summary>
        /// Gets the buffer compressor.
        /// </summary>
        public Action<IList<TObject>> BufferCompressor { get; private set; }

        private readonly AutoResetEvent bufferHasItems = new AutoResetEvent(false);
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CrossThreadPipe&lt;TObject&gt;"/> class.
        /// </summary>
        /// <param name="closeTimeout">The close timeout.</param>
        /// <param name="bufferCompressor">The buffer compressor callback. It is executed synchronized, 
        /// so there is no need for synchronization inside.
        /// </param>
        public CrossThreadPipe(TimeSpan closeTimeout, Action<IList<TObject>> bufferCompressor = null)
        {
            Buffer = null;
            CloseTimeout = closeTimeout;
            BufferCompressor = bufferCompressor;
        }

        /// <summary>
        /// Publishes the specified object to the waiting subscriber or
        /// stores it in internal cache.
        /// </summary>
        /// <param name="object">The @object.</param>
        public void Publish(TObject @object)
        {
            Action action = () =>
                                {
                                    lock (bufferHasItems)
                                    {
                                        if (isClosed && noMorePublishingIsAllowed)
                                            return;

                                        (Buffer ?? (Buffer = new List<TObject>())).Add(@object);
                                        if (BufferCompressor != null)
                                            BufferCompressor(Buffer);

                                        bufferHasItems.Set();
                                    }
                                };
            lock (bufferHasItems)
            {
                latterTask = latterTask != null
                                        ? latterTask.ContinueWith(task => action())
                                        : Task.Factory.StartNew(action);
                
            }
        }

        private bool isClosed;
        private bool noMorePublishingIsAllowed;
        private volatile Task latterTask = null; 

        // for preventing garbage collection
        private Timer timer;

        /// <summary>
        /// Closes this instance disallowing to publish any more results
        /// and removing the last waiting results after the closeTimeout
        /// time span.
        /// </summary>
        public void Close()
        {
            Action closeAction = () =>
                                {
                                    lock (bufferHasItems)
                                    {
                                        isClosed = true;
                                        bufferHasItems.Set();

                                        // for preventing garbage collection
                                        timer = new Timer(ClearBufferAfterCloseTimeout, null, CloseTimeout, TimeSpan.FromMilliseconds(Timeout.Infinite));
                                    }
                                };

            lock (bufferHasItems)
            {
                latterTask = latterTask != null
                                        ? latterTask.ContinueWith(task => closeAction())
                                        : Task.Factory.StartNew(closeAction);

            }
        }

        private void ClearBufferAfterCloseTimeout(object state)
        {
            lock(bufferHasItems)
            {
                Buffer = null;
                noMorePublishingIsAllowed = true;
            }
        }

        /// <summary>
        /// Gets the available results. If there are no results available they are stored temporary.
        /// </summary>
        /// <returns></returns>
        public IList<TObject> GetAvailableResults()
        {
            lock (bufferHasItems)
            {
                if (isClosed)
                {
                    noMorePublishingIsAllowed = true;
                    return GetBufferAndReplaceIt();
                }
            }

            do
            {
                bufferHasItems.WaitOne();
                var taskWeWaitFor = latterTask;
                if (taskWeWaitFor != null)
                {
                    taskWeWaitFor.Wait();
                    lock (bufferHasItems)
                    {
                        if (taskWeWaitFor == latterTask)
                            break;
                    }
                }
                else
                {
                    break;
                }
            } 
            while (true);
            
            lock(bufferHasItems)
            {
                // clear the event, if some other threads called
                // the event.Set() inbetween
                bufferHasItems.Reset();

                if (isClosed)
                    noMorePublishingIsAllowed = true;

                return GetBufferAndReplaceIt();
            }
        }

        private IList<TObject> GetBufferAndReplaceIt()
        {
            var oldBuffer = Buffer;
            Buffer = null;
            return oldBuffer;
        }
    }
}