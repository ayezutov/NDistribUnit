using System.Collections.Generic;
using NDistribUnit.Common.ConsoleProcessing.Options;

namespace NDistribUnit.Common.Agent
{
    /// <summary>
    /// 
    /// </summary>
    public class AgentParameters
    {
        /// <summary>
        /// Parses the command line returning a typed 
        /// options object
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static AgentParameters Parse(IEnumerable<string> arguments)
        {
            var result = new AgentParameters();
            var set = new ConsoleParametersParser
                {
                    {"port", (int port) => result.Port = port},
                };

            set.Parse(arguments);
            return result;
        }

        /// <summary>
        /// Gets the port.
        /// </summary>
        public int? Port { get; private set; }
    }
}