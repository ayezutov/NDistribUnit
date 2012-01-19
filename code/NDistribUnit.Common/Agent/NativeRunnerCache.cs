using System;
using System.Collections.Concurrent;
using System.Threading;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.TestExecution.DistributedConfiguration;
using NUnit.Core;

namespace NDistribUnit.Common.Agent
{
    /// <summary>
    /// 
    /// </summary>
    public class NativeRunnerCache : INativeRunnerCache
    {
        private class TestRunnerMetadata
        {
            public TestRunner Runner { get; set; }
            public Timer Timer { get; set; }
        }

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, TestRunnerMetadata>> cache = new ConcurrentDictionary<string, ConcurrentDictionary<int, TestRunnerMetadata>>();

        /// <summary>
        /// Gets the or load.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <param name="substitutions"></param>
        /// <param name="loadRunner">The action.</param>
        /// <returns></returns>
        public TestRunner GetOrLoad(TestRun testRun, DistributedConfigurationSubstitutions substitutions, Func<TestRunner> loadRunner)
        {
            var key = GetKey(testRun);
            var timeSpan = TimeSpan.FromMinutes(59);
            var cacheByParameters = cache.GetOrAdd(key, guid => new ConcurrentDictionary<int, TestRunnerMetadata>());
            int hashCode = substitutions == null ? 0 : substitutions.GetHashCode();

            var metadata = cacheByParameters.GetOrAdd(hashCode, hash =>
                                                 {
                                                     var runner = loadRunner();
                                                     var timer = new Timer(obj=>
                                                                               {
                                                                                   TestRunnerMetadata removed;
                                                                                   if (cacheByParameters.TryRemove(hashCode, out removed))
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
            ConcurrentDictionary<int, TestRunnerMetadata> removed;
            if (cache.TryRemove(GetKey(run), out removed))
            {
                foreach (var testRunnerMetadata in removed)
                {
                    testRunnerMetadata.Value.Timer.Change(Timeout.Infinite, Timeout.Infinite);
                    testRunnerMetadata.Value.Runner.Unload(); 
                }
            }
        }

        private static string GetKey(TestRun testRun)
        {
            return string.IsNullOrEmpty(testRun.Alias) ? testRun.Id.ToString() : testRun.Alias;
        }
    }
}