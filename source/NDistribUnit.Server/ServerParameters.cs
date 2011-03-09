using System;
using System.Collections.Generic;

namespace NDistribUnit.Server
{
    internal class ServerParameters
    {
        public static ServerParameters Parse(IEnumerable<string> arguments)
        {
            var result = new ServerParameters()
                             {
                                 DashboardPort = 8008,
                                 TestRunnerPort = 8009
                             };
            return result;
        }

        public int DashboardPort { get; private set; }

        public int TestRunnerPort { get; private set; }
    }
}