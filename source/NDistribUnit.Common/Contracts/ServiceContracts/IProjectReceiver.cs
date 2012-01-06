using System.ServiceModel;
using NDistribUnit.Common.Contracts.DataContracts;

namespace NDistribUnit.Common.Contracts.ServiceContracts
{
    /// <summary>
    /// Represents a project receiver
    /// </summary>
    public interface IProjectReceiver
    {
        /// <summary>
        /// Receives the project.
        /// </summary>
        /// <param name="project">The project.</param>
        [OperationContract]
        void ReceiveProject(ProjectMessage project);

        /// <summary>
        /// Receives the project.
        /// </summary>
        /// <param name="run">The run.</param>
        /// <returns>
        ///   <c>true</c> if the specified agent has a project for the given run; otherwise, <c>false</c>.
        /// </returns>
        [OperationContract]
        bool HasProject(TestRun run);
    }
}