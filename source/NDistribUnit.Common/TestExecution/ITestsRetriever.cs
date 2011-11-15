using System.Collections.Generic;
using NDistribUnit.Common.TestExecution.Data;
using NDistribUnit.Common.TestExecution.Storage;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITestsRetriever
    {
        /// <summary>
        /// Parses the specified project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        IEnumerable<TestUnitWithMetadata> Get(TestProject project, TestRunRequest request);
    }
}