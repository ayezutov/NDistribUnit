using System;
using Autofac;
using NDistribUnit.Agent.Communication;
using NDistribUnit.Agent.Options;
using NDistribUnit.Common.ConsoleProcessing;

namespace NDistribUnit.Agent
{
    public class AgentProgram
    {
        public AgentHost AgentHost { get; set; }

        static int Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<AgentProgram>();
            builder.RegisterType<AgentHost>();
            builder.Register(c => AgentConsoleParameters.Parse(args)).InstancePerLifetimeScope();
            var container = builder.Build();
            return container.Resolve<AgentProgram>().Run();
        }

        public AgentProgram(AgentHost agentHost)
        {
            AgentHost = agentHost;
        }

        private int Run()
        {
            Console.WriteLine("Starting...");
            AgentHost.Start();
            Console.WriteLine("Started. Press <Enter> to exit.");
            Console.ReadLine();
            AgentHost.Stop();
            return 0;
        }
    }
}
