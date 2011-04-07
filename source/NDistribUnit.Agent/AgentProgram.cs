using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using Autofac;
using NDistribUnit.Agent.Options;
using NDistribUnit.Common.Agent;
using NDistribUnit.Common.Agent.ExternalModules;
using NDistribUnit.Common.Agent.Naming;
using NDistribUnit.Common.Logging;

namespace NDistribUnit.Agent
{
    /// <summary>
    /// The entry point into agent's console application
    /// </summary>
    public class AgentProgram
    {
        private readonly ILog log;

        /// <summary>
        ///  The host, which enables all communication services
        /// </summary>
        public AgentHost AgentHost { get; set; }

        static int Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<AgentProgram>();
            builder.Register(c => new RollingLog(1000)).InstancePerLifetimeScope();
            builder.Register(c => new CombinedLog(new ConsoleLog(), c.Resolve<RollingLog>())).As<ILog>();
            builder.Register(c => new AgentHost(new TestRunnerAgentService(c.Resolve<ILog>(), string.Format("{0} #{1:000}", Environment.MachineName, InstanceNumberSearcher.Number), c.Resolve<RollingLog>()), new IAgentExternalModule[]
                                                                                          {
                                                                                              new DiscoveryModule(new Uri("http://hubwoo.com/trr-odc")),
                                                                                              new AnnouncementModule(TimeSpan.FromSeconds(15), new Uri("http://hubwoo.com/trr-odc"), c.Resolve<ILog>())
                                                                                          }, c.Resolve<ILog>()));
            builder.Register(c => AgentConsoleParameters.Parse(args)).InstancePerLifetimeScope();
            var container = builder.Build();
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            return container.Resolve<AgentProgram>().Run();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of an agent program
        /// </summary>
        /// <param name="agentHost"></param>
        /// <param name="log"></param>
        public AgentProgram(AgentHost agentHost, ILog log)
        {
            this.log = log;
            AgentHost = agentHost;
        }


        private int Run()
        {
            try
            {
                log.BeginActivity("Starting agent...");
                AgentHost.Start();
                log.EndActivity("Agent was successfully started. Press <Enter> to exit.");


                Console.ReadLine();
                AgentHost.Stop();
            }
            catch (Exception ex)
            {
                log.Error("Exception, while running", ex);
                throw;
            }
            
            
            return 0;
        }
    }
}
