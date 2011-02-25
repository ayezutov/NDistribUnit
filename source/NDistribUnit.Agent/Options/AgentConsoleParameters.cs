using System;
using System.Collections.Generic;
using NDistribUnit.Common.ConsoleProcessing.Options;

namespace NDistribUnit.Agent.Options
{
    public class AgentConsoleParameters
    {
        public int Port { get; set; }
        public static AgentConsoleParameters Parse(IEnumerable<string> arguments)
        {
            var result = new AgentConsoleParameters();
            new ConsoleParametersParser
                {
                    {"port", (int port) => result.Port = port}
                }.Parse(arguments);
            return result;
        }
    }
}