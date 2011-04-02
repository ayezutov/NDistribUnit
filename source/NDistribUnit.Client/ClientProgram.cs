using System;
using Autofac;
using NDistribUnit.Client.Configuration;
using NDistribUnit.Common.Logging;

namespace NDistribUnit.Client
{
    /// <summary>
    /// The entry point for the client
    /// </summary>
    public class ClientProgram
    {
        private readonly ILog log;

        static int Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ClientProgram>();
            builder.RegisterType<ConsoleLog>().As<ILog>();
            builder.Register(c => ClientParameters.Parse(args)).InstancePerLifetimeScope();

            var container = builder.Build();

            return container.Resolve<ClientProgram>().Run();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientProgram"/> class.
        /// </summary>
        /// <param name="options">Options, which were provided through command line</param>
        /// <param name="log">The log.</param>
        public ClientProgram(ClientParameters options, ILog log)
        {
            this.log = log;
        }

        /// <summary>
        /// Runs the program with specified options
        /// </summary>
        /// <returns>A return code</returns>
        protected int Run()
        {
            log.EndActivity("Client was started");
            Console.ReadLine();
            return 0;
        }
    }
}
