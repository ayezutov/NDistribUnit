using System.IO;
using System.ServiceModel;
using NDistribUnit.Common.Contracts.DataContracts;

namespace NDistribUnit.Common.Contracts.ServiceContracts
{
    /// <summary>
    /// 
    /// </summary>
    [MessageContract]
    public class ProjectMessage
    {
        /// <summary>
        /// Gets or sets the test run.
        /// </summary>
        /// <value>
        /// The test run.
        /// </value>
        [MessageHeader]
        public TestRun TestRun { get; set; }

        /// <summary>
        /// Gets or sets the project.
        /// </summary>
        /// <value>
        /// The project.
        /// </value>
        [MessageBodyMember]
        public Stream Project { get; set; }
    }
}