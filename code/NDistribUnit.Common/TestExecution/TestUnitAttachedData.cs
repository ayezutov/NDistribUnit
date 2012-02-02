using System.Collections.Concurrent;
using NDistribUnit.Common.Server.AgentsTracking;
using NDistribUnit.Common.TestExecution.Configuration;
using NDistribUnit.Common.TestExecution.Scheduling;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// Holds the data, which is required for the scheduler to make right decisions
    /// </summary>
    public class TestUnitAttachedData
    {
        /// <summary>
        /// Gets or sets the number of time a reprocessing occurred for null.
        /// </summary>
        /// <value>
        /// The null reprocessing count.
        /// </value>
        public int NullReprocessingCount { get; set; }

        readonly ConcurrentDictionary<TestRunFailureSpecialHandling, int>  specialHandlingsEntryCount = new ConcurrentDictionary<TestRunFailureSpecialHandling, int>();

        /// <summary>
        /// Gets the count and increase.
        /// </summary>
        /// <param name="specialHandling">The special handling.</param>
        /// <returns></returns>
        public int GetCountAndIncrease(TestRunFailureSpecialHandling specialHandling)
        {
            return specialHandlingsEntryCount.AddOrUpdate(specialHandling, 1, (handling, i) => ++i);
        }

        /// <summary>
        /// Marks the agent as.
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <param name="schedulingHint">The scheduling hint.</param>
        public void MarkAgentAs(AgentMetadata agent, SchedulingHint schedulingHint)
        {            
        }
    }
}