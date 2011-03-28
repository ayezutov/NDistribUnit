using System;
using Autofac;
using NDistribUnit.Agent.Communication;
using NDistribUnit.Agent.Options;

namespace NDistribUnit.Agent
{
    /// <summary>
    /// The entry point into agent's console application
    /// </summary>
    public class AgentProgram
    {
        /// <summary>
        ///  The host, which enables all communication services
        /// </summary>
        public AgentHost AgentHost { get; set; }

        static int Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<AgentProgram>();
            builder.Register(c => new AgentHost(new Uri("http://hubwoo.com/trr-odc")));
            builder.Register(c => AgentConsoleParameters.Parse(args)).InstancePerLifetimeScope();
            var container = builder.Build();
            return container.Resolve<AgentProgram>().Run();
        }

        /// <summary>
        /// Initializes a new instance of an agent program
        /// </summary>
        /// <param name="agentHost"></param>
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
