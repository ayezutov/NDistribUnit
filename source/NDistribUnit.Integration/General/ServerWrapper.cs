﻿using System;
using NDistribUnit.Server;
using NDistribUnit.Server.Communication;

namespace NDistribUnit.Integration.Tests.General
{
    /// <summary>
    /// The class is a special wrapper around real server program to allow easy access
    /// in testing code
    /// </summary>
    public class ServerWrapper
    {
        private readonly ServerHost serverHost;

        private ServerWrapper(ServerHost serverHost)
        {
            this.serverHost = serverHost;
        }

        public static ServerWrapper Any { get; private set; }

        public static ServerWrapper Start()
        {
            var host = new ServerHost(9008, 9009);
            var serverWrapper = new ServerWrapper(host);
            host.Start();
            return serverWrapper;
        }

        public bool IsConnectedTo(AgentWrapper agent)
        {
            throw new NotImplementedException();
        }

        public bool IsNotConnectedTo(AgentWrapper agent)
        {
            return !IsConnectedTo(agent);
        }

        public void ShutDownInExpectedWay()
        {
            serverHost.Close();
        }

        public void ShutDownUngraceful()
        {
            throw new NotImplementedException();
        }
    }
}