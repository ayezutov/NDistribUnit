using System;
using System.Diagnostics;
using System.Threading;
using NDistribUnit.Common.Logging;

namespace NDistribUnit.Common.Common.ConsoleProcessing
{
    /// <summary>
    /// 
    /// </summary>
    public class ExceptionCatcher
    {
        private ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionCatcher"/> class.
        /// </summary>
        /// <param name="log">The log.</param>
        public ExceptionCatcher(ILog log)
        {
            this.log = log;
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        public void Run(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private void LogException(Exception ex)
        {
            log.Error(
                string.Format("Unhandled exception encountered in thread '{0}' of domain '{1}'", Thread.CurrentThread.Name,
                              AppDomain.CurrentDomain.FriendlyName), ex);
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        public TResult Run<TResult>(Func<TResult> action)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                LogException(ex);
                return default(TResult);
            }
        }
    }
}