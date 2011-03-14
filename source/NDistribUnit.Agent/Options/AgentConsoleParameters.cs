using System.Collections.Generic;
using NDistribUnit.Common.ConsoleProcessing.Options;

namespace NDistribUnit.Agent.Options
{
    /// <summary>
    /// The console parameters of the agent
    /// </summary>
    public class AgentConsoleParameters
    {
        /// <summary>
        /// Port, which the agent should be run on
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Parses the command line parameters
        /// </summary>
        /// <param name="arguments">Command line parameters</param>
        /// <returns>Parsed console parameters</returns>
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