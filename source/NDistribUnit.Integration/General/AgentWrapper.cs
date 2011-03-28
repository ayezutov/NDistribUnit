using System;
using NDistribUnit.Agent;
using NDistribUnit.Agent.Communication;

namespace NDistribUnit.Integration.Tests.General
{
    public class AgentWrapper
    {
        internal AgentWrapper(AgentHost agentHost)
        {
            AgentHost = agentHost;
        }

        public AgentHost AgentHost { get; private set; }

        public void Start()
        {
            AgentHost.Start();
        }

        public void ShutDownInExpectedWay()
        {
            AgentHost.Stop();
        }

        public void ShutDownUngraceful()
        {
            AgentHost.Abort();
        }

        public void Dispose()
        {
            AgentHost.Abort();
        }
    }
}