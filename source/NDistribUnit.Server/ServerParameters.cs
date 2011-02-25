using System.Collections.Generic;

namespace NDistribUnit.Server
{
    internal class ServerParameters
    {
        public static ServerParameters Parse(IEnumerable<string> arguments)
        {
            return new ServerParameters();
        }
    }
}