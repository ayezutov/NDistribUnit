using System;
using System.Collections.Concurrent;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.TestExecution.DistributedConfiguration;
using NDistribUnit.Common.TestExecution.Exceptions;
using NDistribUnit.Common.TestExecution.Storage;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public class TestsScheduler : ITestsScheduler
    {
        private readonly TestAgentsCollection agents;
        private readonly TestUnitsCollection tests;
        private readonly IRequestsStorage requests;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestsScheduler"/> class.
        /// </summary>
        /// <param name="agents">The agents.</param>
        /// <param name="tests">The tests.</param>
        /// <param name="requests">The requests.</param>
        public TestsScheduler(TestAgentsCollection agents, TestUnitsCollection tests, IRequestsStorage requests)
        {
            this.agents = agents;
            this.tests = tests;
            this.requests = requests;
        }

        /// <summary>
        /// Gets the agent and test.
        /// </summary>
        /// <returns></returns>
        public Tuple<AgentInformation, TestUnitWithMetadata, DistributedConfigurationSubstitutions> GetAgentAndTestAndVariables()
        {
            lock (tests.SyncObject)
            {
                var availableTests = tests.GetAvailable();
                if (availableTests.Count == 0)
                    return null;

                var testToRun = availableTests[0];

                lock (agents.SyncObject)
                {
                    if (!agents.AreConnectedAvailable)
                        throw new NoAvailableAgentsException();

                    var freeAgents = agents.GetFree();

                    if (freeAgents.Count == 0)
                        return null;

                    var agentToRun = freeAgents[0];

                    var request = requests.GetBy(testToRun.Test.Run);
                    if (request.ConfigurationSetup == null)
                        return new Tuple<AgentInformation, TestUnitWithMetadata, DistributedConfigurationSubstitutions>(
                            agentToRun,
                            testToRun,
                            null);


                    DistributedConfigurationSubstitutions configSubstitutions = GetConfigurationValues(request.ConfigurationSetup,
                                                                                         agentToRun, testToRun);
                    if (configSubstitutions == null)
                        return null;

                    return new Tuple<AgentInformation, TestUnitWithMetadata, DistributedConfigurationSubstitutions>(
                        agentToRun,
                        testToRun,
                        configSubstitutions);
                }
            }
        }

        private ConcurrentDictionary<string, DistributedConfigurationSubstitutions> configurations =
            new ConcurrentDictionary<string, DistributedConfigurationSubstitutions>();

        private DistributedConfigurationSubstitutions GetConfigurationValues(DistributedConfigurationSetup configurationSetup,
                                                                      AgentInformation agentToRun,
                                                                      TestUnitWithMetadata testToRun)
        {
            return configurations.GetOrAdd(agentToRun.Name,
                                    key =>
                                        {
                                            var distributedConfigurationValues = new DistributedConfigurationSubstitutions();

                                            foreach (var variable in configurationSetup.Variables)
                                            {
                                                distributedConfigurationValues.Variables.Add(
                                                    new DistributedConfigurationVariablesValue(variable.Name, variable.GetNextValue()));
                                            }

                                            return distributedConfigurationValues;
                                        });
        }
    }
}