using System;
using System.Threading;

namespace NDistribUnit.Common.Retrying
{
    /// <summary>
    /// A set of utilities, used to implement retrying logic
    /// </summary>
    public static class Retry
    {
        /// <summary>
        /// Retries to evaluate condition, until the condition is not met.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="maxCount">The max count.</param>
        /// <returns></returns>
        public static bool While(Func<bool> condition, int interval, int maxCount = 10)
        {
            var count = 0;
            while (!condition())
            {
                if (count++ >= maxCount)
                    return false;

                Thread.Sleep(interval);
            }
            return true;
        }
    }
}
