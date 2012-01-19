using System.Runtime.Serialization;

namespace NDistribUnit.Common.DataContracts
{
    /// <summary>
    /// The project description
    /// </summary>
    [DataContract]
    public class ProjectDescription
    {
        /// <summary>
        /// Gets or sets project unique identifier
        /// </summary>
        [DataMember]
        public string UniqueIdentifier { get; set; }

        /// <summary>
        /// Project friendly name
        /// </summary>
        [DataMember]
        public string Name { get; set; }
    }
}