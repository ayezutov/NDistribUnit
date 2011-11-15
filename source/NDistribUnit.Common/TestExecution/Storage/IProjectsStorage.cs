using System;
using NDistribUnit.Common.Contracts.DataContracts;

namespace NDistribUnit.Common.TestExecution.Storage
{
    /// <summary>
    /// The storage on server or agent
    /// </summary>
    public interface IProjectsStorage
    {
        /// <summary>
        /// Gets the project.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <param name="loadPackedProject">The load packed project.</param>
        /// <returns></returns>
        TestProject GetOrLoad(TestRun testRun, Func<PackedProject> loadPackedProject = null);

        /// <summary>
        /// Gets the packed project.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <returns></returns>
        PackedProject GetPackedProject(TestRun testRun);

        /// <summary>
        /// Stores the specified project test run.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        TestProject Store(TestRun testRun, PackedProject project);
    }
}