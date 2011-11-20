using NDistribUnit.Common.Contracts.DataContracts;
using NUnit.Core;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// A reprocessing strategy
    /// </summary>
    public class Reprocessor : IReprocessor
    {
        /// <summary>
        /// Adds for reprocessing if required.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="result">The result.</param>
        public void AddForReprocessingIfRequired(TestUnitWithMetadata test, TestUnitResult result)
        {}
    }
}