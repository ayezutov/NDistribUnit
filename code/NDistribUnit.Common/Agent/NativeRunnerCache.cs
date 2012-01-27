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
            //public Timer Timer { get; set; }
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
            //var timeSpan = TimeSpan.FromHours(4);
            var cacheByParameters = cache.GetOrAdd(key, guid => new ConcurrentDictionary<int, TestRunnerMetadata>());
            int hashCode = GetKeyForSubstitutions(substitutions);

            var metadata = cacheByParameters.GetOrAdd(hashCode, hash =>
                                                 {
                                                     var runner = loadRunner();
//                                                     var timer = new Timer(obj=>
//                                                                               {
//                                                                                   TestRunnerMetadata removed;
//                                                                                   if (cacheByParameters.TryRemove(hashCode, out removed))
//                                                                                       removed.Runner.Unload();
//                                                                               }, null, timeSpan, TimeSpan.FromMilliseconds(-1));
                                                     return new TestRunnerMetadata
                                                                {
                                                                    Runner = runner,
//                                                                    Timer = timer
                                                                };
                                                 });
//            metadata.Timer.Change(timeSpan, TimeSpan.FromMilliseconds(-1));
            return metadata.Runner;
        }

        private static int GetKeyForSubstitutions(DistributedConfigurationSubstitutions substitutions)
        {
            return substitutions == null ? 0 : substitutions.GetHashCode();
        }

        /// <summary>
        /// Removes the cached runner for the specified run and configuration.
        /// </summary>
        /// <param name="run">The run.</param>
        /// <param name="configurationSubstitutions">The configuration substitutions.</param>
        public void Remove(TestRun run, DistributedConfigurationSubstitutions configurationSubstitutions)
        {
            ConcurrentDictionary<int, TestRunnerMetadata> byParametersCache;
            if (cache.TryGetValue(GetKey(run), out byParametersCache))
            {
                TestRunnerMetadata metadata;
                if (byParametersCache.TryRemove(GetKeyForSubstitutions(configurationSubstitutions), out metadata))
                {
                    metadata.Runner.Unload();
                }
            }
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
                foreach (var testRunnerMetadata in removed.Values)
                {
//                    testRunnerMetadata.Value.Timer.Change(Timeout.Infinite, Timeout.Infinite);
                    testRunnerMetadata.Runner.Unload(); 
                }
            }
        }

        private static string GetKey(TestRun testRun)
        {
            return /*string.IsNullOrEmpty(testRun.Alias) ?*/ testRun.Id.ToString() /*: testRun.Alias*/;
        }
    }
}