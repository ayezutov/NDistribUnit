using System;
using NUnit.Core;

namespace NDistribUnit.Common.Contracts.DataContracts
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class TestUnitResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestUnitResult"/> class.
        /// </summary>
        /// <param name="result">The result.</param>
        public TestUnitResult(TestResult result)
        {
            Result = result;
        }

        /// <summary>
        /// Gets or sets the assert count.
        /// </summary>
        /// <value>
        /// The assert count.
        /// </value>
        public TestResult Result { get; private set; }
    }
}