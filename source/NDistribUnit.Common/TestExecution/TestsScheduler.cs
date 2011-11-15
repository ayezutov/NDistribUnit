using System;
using NDistribUnit.Common.Contracts.DataContracts;
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
        public Tuple<AgentInformation, TestUnitWithMetadata> GetAgentAndTest()
        {
            lock (tests.SyncObject)
            {
                var availableTests = tests.GetAvailable();
                if (availableTests.Count == 0)
                    return null;

                lock (agents.SyncObject)
                {
                    if (!agents.AreConnectedAvailable)
                        throw new NoAvailableAgentsException();

                    var freeAgents = agents.GetFree();

                    if (freeAgents.Count == 0)
                        return null;

                    return new Tuple<AgentInformation, TestUnitWithMetadata>(freeAgents[0], availableTests[0]);
                }
            }
        }
    }
}