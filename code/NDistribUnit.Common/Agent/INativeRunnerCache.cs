using System;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.TestExecution.DistributedConfiguration;
using NUnit.Core;

namespace NDistribUnit.Common.Agent
{
    /// <summary>
    /// 
    /// </summary>
    public interface INativeRunnerCache
    {
        /// <summary>
        /// Gets the or load.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <param name="substitutions"></param>
        /// <param name="loadRunner">The action.</param>
        /// <returns></returns>
        TestRunner GetOrLoad(TestRun testRun, DistributedConfigurationSubstitutions substitutions, Func<TestRunner> loadRunner);

        /// <summary>
        /// Removes the specified run.
        /// </summary>
        /// <param name="run">The run.</param>
        /// <param name="configurationSubstitutions">The configuration substitutions.</param>
        void Remove(TestRun run, DistributedConfigurationSubstitutions configurationSubstitutions = null);
    }
}