using System;
using System.ServiceModel.Discovery;
using System.Threading;
using NDistribUnit.Common.ServiceContracts;

namespace NDistribUnit.Common.Communication
{
    /// <summary>
    /// The endpoint information, which is being tracked
    /// </summary>
    public class EndpointInformation
    {
        /// <summary>
        /// The pingable channel
        /// </summary>
        public IPingable Pingable { get; set; }

        /// <summary>
        /// The endpoint metadata
        /// </summary>
        public EndpointDiscoveryMetadata Endpoint { get; set; }

        /// <summary>
        /// The timer, which is fired, when a ping should be performed
        /// </summary>
        public Timer PingTimer { get; set; }

        /// <summary>
        /// The most recent time, when the endpoint's state was updated
        /// </summary>
        public DateTime LastStatusUpdateTime { get; set; }

        private bool Equals(EndpointInformation other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Endpoint.Address.ToString(), Endpoint.Address.ToString());
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (EndpointInformation)) return false;
            return Equals((EndpointInformation) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return (Endpoint != null ? Endpoint.Address.GetHashCode() : 0);
        }
    }
}