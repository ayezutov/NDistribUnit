using System;
using System.ServiceModel;
using Autofac;
using NDistribUnit.Common.Client;
using NDistribUnit.Common.Common;
using NDistribUnit.Common.Common.ConsoleProcessing;
using NDistribUnit.Common.Common.Logging;
using NDistribUnit.Common.Communication;
using NDistribUnit.Common.Dependencies;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Client
{
    /// <summary>
    /// The entry point for the client
    /// </summary>
    public class ClientProgram : GeneralProgram
    {
        private readonly ClientParameters options;
        private readonly Common.Client.Client client;

        static int Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.Register(c => ClientParameters.Parse(args)).InstancePerLifetimeScope();
            builder.Register(c => new LogConfiguration { RollingLogItemsCount = 1000 }).InstancePerLifetimeScope();
            builder.RegisterType<ClientProgram>();
            builder.RegisterModule(new ClientDependenciesModule());
            builder.RegisterModule(new CommonDependenciesModule(args));
            var container = builder.Build();
            try
            {
                return container.Resolve<ClientProgram>().Run();
            }
            catch (Exception ex)
            {
                var log = container.Resolve<ConsoleLog>();
                log.Error("Some error, while running tests", ex);
                Console.WriteLine("Please press any key to continue");
                Console.ReadKey();
                return (int)ReturnCodes.UnhandledException;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientProgram"/> class.
        /// </summary>
        /// <param name="options">Options, which were provided through command line</param>
        /// <param name="bootstrapperParameters">The bootstrapper parameters.</param>
        /// <param name="client">The client.</param>
        /// <param name="resolver">The resolver.</param>
        /// <param name="catcher">The catcher.</param>
        /// <param name="log">The log.</param>
        public ClientProgram(ClientParameters options, BootstrapperParameters bootstrapperParameters, Common.Client.Client client, AssemblyResolver resolver, ExceptionCatcher catcher, ILog log)
            : base(resolver, null, catcher, log, bootstrapperParameters)
        {
            this.options = options;
            this.client = client;
        }

        /// <summary>
        /// Runs the program with specified options
        /// </summary>
        /// <returns>A return code</returns>
        private int Run()
        {
            if (!bootstrapperParameters.AllParametersAreFilled)
            {
                log.Error("Bootstrapped application cannot be launched directly");
                return (int)ReturnCodes.CannotLaunchBootstrappedApplicationDirectly;
            }

            log.EndActivity("Client was started");

            log.BeginActivity(string.Format("Starting running test: {0}" +
                                            "\tassemblies   : '{1}'{0}" +
                                            "\ton           : '{2}'{0}" +
                                            "\tconfiguration: '{3}'{0}" +
                                            "\toutput file  : '{4}'{0}" +
                                            "\ttest to run  : '{5}'{0}" +
                                            "\tinclude      : '{6}'{0}" +
                                            "\texclude      : '{7}'{0}"
                                            ,
                                            Environment.NewLine, 
                                            string.Join(";", options.NUnitParameters.AssembliesToTest),//1
                                            options.ServerUri, //2
                                            options.NUnitParameters.Configuration,//3
                                            options.NUnitParameters.XmlFileName,//4
                                            options.NUnitParameters.TestToRun,//5
                                            options.NUnitParameters.IncludeCategories,//6
                                            options.NUnitParameters.ExcludeCategories//7
                                            ));
            if (options.ServerUri == null)
            {
                log.Error("Please provide server Uri");
                return (int)ReturnCodes.IncompleteParameterList;
            }


            int failsAndErrorsCount;
            try
            {
                log.BeginActivity("Running tests on server...");
                failsAndErrorsCount = client.Run();
                log.EndActivity("Test run was completed");
            }
            catch (CommunicationException e)
            {
                log.Error("Server seems to be unreachable", e);
                return (int)ReturnCodes.IncompleteParameterList;
            }

            Console.Write("Test run was finished");
            return failsAndErrorsCount;
        }
    }
}
