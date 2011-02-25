using System;
using NDistribUnit.Client.Configuration;
using NDistribUnit.Common.ConsoleProcessing;

namespace NDistribUnit.Client
{
    public class ClientProgram
    {
        static int Main(string[] args)
        {
            return new ClientProgram().Run(ClientParameters.Parse(args));
        }

        protected int Run(ClientParameters options)
        {
            Console.WriteLine("Client was started");
            Console.ReadLine();

            return 0;
        }
    }
}
