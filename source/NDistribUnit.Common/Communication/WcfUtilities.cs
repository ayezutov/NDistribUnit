using System.Net;
using System.Net.Sockets;

namespace NDistribUnit.Server
{
    public static class WcfUtilities
    {
        public static int FindPort()
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 0);

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(endPoint);
                var local = (IPEndPoint)socket.LocalEndPoint;
                return local.Port;
            }
        }
    }
}