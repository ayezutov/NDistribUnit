using System;
using System.Collections.Concurrent;
using System.Threading;
using NDistribUnit.Common.Contracts.DataContracts;
using NUnit.Core;

namespace NDistribUnit.Common.Agent
{
    class NativeRunnerCache : INativeRunnerCache
    {
        private class TestRunnerMetadata
        {
            public TestRunner Runner { get; set; }
            public Timer Timer { get; set; }
        }

        private readonly ConcurrentDictionary<string, TestRunnerMetadata> cache = new ConcurrentDictionary<string, TestRunnerMetadata>();

        /// <summary>
        /// Gets the or load.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <param name="loadRunner">The action.</param>
        /// <returns></returns>
        public TestRunner GetOrLoad(TestRun testRun, Func<TestRunner> loadRunner)
        {
            var key = GetKey(testRun);
            var timeSpan = TimeSpan.FromMinutes(59);
            var metadata = cache.GetOrAdd(key, guid=>
                                                 {
                                                     var runner = loadRunner();
                                                     var timer = new Timer(obj=>
                                                                               {
                                                                                   TestRunnerMetadata removed;
                                                                                   if (cache.TryRemove(key, out removed))
                                                                                       removed.Runner.Unload();
                                                                               }, null, timeSpan, TimeSpan.FromMilliseconds(-1));
                                                     return new TestRunnerMetadata
                                                                {
                                                                    Runner = runner,
                                                                    Timer = timer
                                                                };
                                                 });
            metadata.Timer.Change(timeSpan, TimeSpan.FromMilliseconds(-1));
            return metadata.Runner;
        }

        /// <summary>
        /// Clears the specified run.
        /// </summary>
        /// <param name="run">The run.</param>
        public void Clear(TestRun run)
        {
            TestRunnerMetadata removed;
            if (cache.TryRemove(GetKey(run), out removed))
            {
                removed.Timer.Change(Timeout.Infinite, Timeout.Infinite);
                removed.Runner.Unload();
            }
        }

        private static string GetKey(TestRun testRun)
        {
            return string.IsNullOrEmpty(testRun.Alias) ? testRun.Id.ToString() : testRun.Alias;
        }
    }
}