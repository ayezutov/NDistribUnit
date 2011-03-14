using System;
using NDistribUnit.Client.Configuration;
using NDistribUnit.Common.ConsoleProcessing;

namespace NDistribUnit.Client
{
    /// <summary>
    /// The entry point for the client
    /// </summary>
    public class ClientProgram
    {
        static int Main(string[] args)
        {
            return new ClientProgram().Run(ClientParameters.Parse(args));
        }

        /// <summary>
        /// Runs the program with specified options
        /// </summary>
        /// <param name="options">Options, which were provided through command line</param>
        /// <returns>A return code</returns>
        protected int Run(ClientParameters options)
        {
            Console.WriteLine("Client was started");
            Console.ReadLine();

            return 0;
        }
    }
}
