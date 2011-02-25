using System.Runtime.Serialization;

namespace NDistribUnit.Server
{
    [DataContract]
    public class ProjectDescription
    {
        [DataMember]
        public string UniqueIdentifier { get; set; }

        [DataMember]
        public string Name { get; set; }
    }
}