using System;
using System.Collections.Generic;
using NDistribUnit.Common.TestExecution.Data;

namespace NDistribUnit.Common.TestExecution.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class NoAvailableAgentsException: Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoAvailableAgentsException"/> class.
        /// </summary>
        /// <param name="tests">The available tests.</param>
        public NoAvailableAgentsException(IEnumerable<TestUnitWithMetadata> tests)
        {
            Tests = tests;
        }

        /// <summary>
        /// Gets or sets the tests.
        /// </summary>
        /// <value>
        /// The tests.
        /// </value>
        public IEnumerable<TestUnitWithMetadata> Tests { get; private set; }
    }
}