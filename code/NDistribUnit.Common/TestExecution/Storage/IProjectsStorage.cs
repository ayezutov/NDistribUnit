using System;
using System.IO;
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
        /// <returns></returns>
        TestProject Get(TestRun testRun);


        /// <summary>
        /// Determines whether the specified test run has project.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <returns>
        ///   <c>true</c> if the specified test run has project; otherwise, <c>false</c>.
        /// </returns>
        bool HasProject(TestRun testRun);

        /// <summary>
        /// Stores the specified test run.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <param name="project">The project.</param>
        void Store(TestRun testRun, Stream project);

        /// <summary>
        /// Gets the stream to packed.
        /// </summary>
        /// <param name="run">The run.</param>
        /// <returns></returns>
        Stream GetStreamToPacked(TestRun run);
    }
}