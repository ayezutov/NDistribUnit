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
        /// <param name="testRun"></param>
        TestProject GetProject(TestRun testRun);

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