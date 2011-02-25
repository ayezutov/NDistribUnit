using System;
using NDistribUnit.Server.Communication;

namespace NDistribUnit.Server
{
    internal class ServerProgram
    {
        private static int Main(string[] args)
        {
            return new ServerProgram().Run(ServerParameters.Parse(args));
        }
        private ServerHost serverHost;

        private int Run(ServerParameters options)
        {
            Console.WriteLine("Server is starting...");
            serverHost = new ServerHost(8008, 8009);
            serverHost.Start();
            Console.WriteLine("Server was started. Please press <Enter> to exit");
            Console.ReadLine();
            return 0;
        }
    }
}