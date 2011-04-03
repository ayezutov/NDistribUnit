using System.Collections.Generic;

namespace NDistribUnit.Common.Server.Options
{
    /// <summary>
    /// Parsed server parameters
    /// </summary>
    public class ServerParameters
    {
        /// <summary>
        /// Parses the specified arguments.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns></returns>
        public static ServerParameters Parse(IEnumerable<string> arguments)
        {
            var result = new ServerParameters()
                             {
                                 DashboardPort = 8008,
                                 TestRunnerPort = 8009
                             };
            return result;
        }

        /// <summary>
        /// Gets the dashboard port.
        /// </summary>
        public int DashboardPort { get; private set; }

        /// <summary>
        /// Gets the test runner port.
        /// </summary>
        public int TestRunnerPort { get; private set; }
    }
}