using System;
using System.Collections.Specialized;
using System.ServiceModel;
using NDistribUnit.Common;
using NDistribUnit.Common.Server.AgentsTracking.AgentsProviders;
using NDistribUnit.Integration.Tests.Infrastructure.Entities;
using NDistribUnit.Common.Extensions;

namespace NDistribUnit.Integration.Tests.Infrastructure.Stubs
{
    internal class TestingAgentsProvider : IAgentsProvider
    {
       
        private NDistribUnitTestSystem controller;

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            foreach (var agent in controller.Agents)
            {
                if (agent.IsStarted)
                {
                    OnAgentStarted(agent, EventArgs.Empty);
                }
                agent.Started += OnAgentStarted;
            }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
        }

        public event EventHandler<EventArgs<EndpointAddress>> Connected;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestingAgentsProvider"/> class.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public TestingAgentsProvider(NDistribUnitTestSystem controller)
        {
            this.controller = controller;
            controller.Agents.CollectionChanged += OnAgentsCollectionChanged;
        }

        void OnAgentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                foreach (AgentWrapper item in e.NewItems)
                {
                    item.Started += OnAgentStarted;
                }
        }
        
        private void OnAgentStarted(object sender, EventArgs e)
        {
            Connected.SafeInvoke(this, GetEndpointConnectionChangedEventArgs((AgentWrapper)sender));
        }

        private EventArgs<EndpointAddress> GetEndpointConnectionChangedEventArgs(AgentWrapper agent)
        {
            return
                new EventArgs<EndpointAddress>(
                    new EndpointAddress("http://testHostName.com/" + agent.TestRunner.Name + "/"));
        }
    }
}