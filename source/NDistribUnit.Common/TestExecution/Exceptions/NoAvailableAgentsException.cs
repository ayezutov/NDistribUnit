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
        /// Gets or sets the tests.
        /// </summary>
        /// <value>
        /// The tests.
        /// </value>
        public IEnumerable<TestUnitWithMetadata> Tests { get; set; }
    }
}