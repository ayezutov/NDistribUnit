using System;
using System.ServiceModel.Discovery;
using System.Threading;

namespace NDistribUnit.Common.Communication
{
    public class EndpointInformation
    {
        public IPingable Pingable { get; set; }

        public EndpointDiscoveryMetadata Endpoint { get; set; }

        public Timer PingTimer { get; set; }

        public DateTime LastStatusUpdateTime { get; set; }


        public bool Equals(EndpointInformation other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Endpoint.Address.ToString(), Endpoint.Address.ToString());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (EndpointInformation)) return false;
            return Equals((EndpointInformation) obj);
        }

        public override int GetHashCode()
        {
            return (Endpoint != null ? Endpoint.Address.GetHashCode() : 0);
        }
    }
}