using System.Collections.Generic;
using NDistribUnit.Common.Contracts.DataContracts;
using NUnit.Core;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public class TestUnitWithMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestUnitWithMetadata"/> class.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <param name="test"></param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="children">The children.</param>
        public TestUnitWithMetadata(TestRun testRun, ITest test, string assemblyName, List<TestUnitWithMetadata> children = null) 
        {
            Children = children ?? new List<TestUnitWithMetadata>();
            Test = new TestUnit(test, testRun, assemblyName);
            Results = new List<TestResult>();
            SchedulerHints = new SchedulerHints();
        }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        public List<TestUnitWithMetadata> Children { get; private set; }


        /// <summary>
        /// Gets the test.
        /// </summary>
        public TestUnit Test { get; private set; }

        /// <summary>
        /// Gets or sets the results.
        /// </summary>
        /// <value>
        /// The results.
        /// </value>
        public List<TestResult> Results { get; private set; }

        /// <summary>
        /// Gets or sets the scheduler hints.
        /// </summary>
        /// <value>
        /// The scheduler hints.
        /// </value>
        public SchedulerHints SchedulerHints { get; private set; }
    }
}