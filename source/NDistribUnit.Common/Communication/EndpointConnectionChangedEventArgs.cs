using System;

namespace NDistribUnit.Common.Communication
{
    public class EndpointConnectionChangedEventArgs : EventArgs
    {
        public EndpointInformation EndpointInfo { get; set; }
    }
}