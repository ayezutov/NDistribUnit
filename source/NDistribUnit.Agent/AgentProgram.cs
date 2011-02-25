using System;
using NDistribUnit.Agent.Communication;
using NDistribUnit.Agent.Options;
using NDistribUnit.Common.ConsoleProcessing;

namespace NDistribUnit.Agent
{
    public class AgentProgram
    {
        static void Main(string[] args)
        {
            new AgentProgram().Run(AgentConsoleParameters.Parse(args));
        }

        protected int Run(AgentConsoleParameters parameters)
        {
            Console.WriteLine("Starting...");
            var agentHost = new AgentHost();
            agentHost.Start();
            Console.WriteLine("Started. Press <Enter> to exit.");
            Console.ReadLine();
            agentHost.Stop();
            return 0;
        }
    }
}
