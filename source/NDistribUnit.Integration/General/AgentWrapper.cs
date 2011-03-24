using System;
using NDistribUnit.Agent;
using NDistribUnit.Agent.Communication;

namespace NDistribUnit.Integration.Tests.General
{
    public class AgentWrapper
    {
        public static AgentWrapper Any { get; private set; }

        private AgentWrapper(AgentHost agentHost)
        {
            AgentHost = agentHost;
        }

        public AgentHost AgentHost { get; private set; }

        public static AgentWrapper Start()
        {
            var agentHost = new AgentHost();
            agentHost.Start();
            return new AgentWrapper(agentHost);
        }

        public bool IsConnectedTo(ServerWrapper server)
        {
            throw new NotImplementedException();
        }

        public bool IsNotConnectedTo(ServerWrapper server)
        {
            return !IsConnectedTo(server);
        }

        public void ShutDownInExpectedWay()
        {
            throw new NotImplementedException();
        }

        public void ShutDownUngraceful()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}