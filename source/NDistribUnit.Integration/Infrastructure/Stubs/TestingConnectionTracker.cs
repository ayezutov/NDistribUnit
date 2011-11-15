using System;
using System.Collections.Specialized;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using NDistribUnit.Common.Communication.ConnectionTracking;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Integration.Tests.Infrastructure.Entities;
using NDistribUnit.Common.Extensions;

namespace NDistribUnit.Integration.Tests.Infrastructure.Stubs
{
    internal class TestingConnectionTracker: INetworkExplorer<IRemoteAppPart>
    {
        public event EventHandler<EndpointConnectionChangedEventArgs> EndpointConnected;
        public event EventHandler<EndpointConnectionChangedEventArgs> EndpointSuccessfulPing;
        public event EventHandler<EndpointConnectionChangedEventArgs> EndpointDisconnected;

        private NDistribUnitTestSystemController controller;
        private bool started;

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            started = true;
            foreach (var agent in controller.Agents)
            {
                if (agent.IsStarted)
                {
                    OnAgentStarted(agent, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            started = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestingConnectionTracker"/> class.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public TestingConnectionTracker(NDistribUnitTestSystemController controller)
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
                    item.Stopped += OnAgentStopped;
                }
        }

        private void OnAgentStopped(object sender, EventArgs e)
        {
            EndpointDisconnected.SafeInvoke(this, GetEndpointConnectionChangedEventArgs((AgentWrapper)sender));
        }

        private void OnAgentStarted(object sender, EventArgs e)
        {
            EndpointConnected.SafeInvoke(this, GetEndpointConnectionChangedEventArgs((AgentWrapper)sender));
        }

        private EndpointConnectionChangedEventArgs GetEndpointConnectionChangedEventArgs(AgentWrapper agent)
        {
            return new EndpointConnectionChangedEventArgs
                       {
                           Version = agent.GetVersion(),
                           EndpointInfo = new EndpointInformation
                                              {
                                                  Name = agent.TestRunner.Name,
                                                  Version = agent.GetVersion(),
                                                  Endpoint = new EndpointDiscoveryMetadata
                                                                 {
                                                                     Address = new EndpointAddress("http://testHostName.com/"+agent.TestRunner.Name+"/")
                                                                 }
                                              }
                       };
        }
    }
}