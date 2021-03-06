﻿using System.Net;
using System.Net.Sockets;

namespace NDistribUnit.Common.Common.Networking
{
    /// <summary>
    /// Common utilities, which could be used for WCF-related tasks
    /// </summary>
    public static class WcfUtilities
    {
        /// <summary>
        /// Finds a free available port on current machine
        /// </summary>
        /// <returns>The free port number</returns>
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
        
        /// <summary>
        /// Finds a free available port on current machine
        /// </summary>
        /// <returns>The free port number</returns>
        public static bool IsFreePort(int port)
        {
            var endPoint = new IPEndPoint(IPAddress.Any, port);

            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Bind(endPoint);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}