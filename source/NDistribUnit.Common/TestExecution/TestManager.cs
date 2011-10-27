using System;
using NDistribUnit.Common.Server.Communication;
using NDistribUnit.Common.Server.Services;
using NDistribUnit.Common.TestExecution.Preparation;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
	public class TestManager
	{
		private readonly ServerConnectionsTracker agents;
		private readonly TestUnitCollection tests;
	    //private readonly TestRunRequestsStorage requests;

	    /// <summary>
		/// 
		/// </summary>
		public readonly object SyncRoot = new object();

	    private TestWorkProvider workProvider;

	    /// <summary>
	    /// Initializes a new instance of the <see cref="TestManager"/> class.
	    /// </summary>
	    /// <param name="agents">The agents.</param>
	    /// <param name="tests">The tests.</param>
	    /// <param name="workProvider">The work provider.</param>
	    public TestManager(ServerConnectionsTracker agents, TestUnitCollection tests, TestWorkProvider workProvider)
		{
			this.agents = agents;
			agents.AgentStateChanged += OnAgentStateChanged;
			this.tests = tests;
            this.workProvider = workProvider;
		}
        
	    private void OnAgentStateChanged(object sender, EventArgs e)
		{
			lock(SyncRoot)
			{
				//agents.GetFreeAgent();
			}
		}

//		/// <summary>
//		/// Occurs when a test should be run on a specific agent.
//		/// </summary>
//		public event Action<TestUnit, AgentInformation> RunTest;
	}


    /// <summary>
    /// 
    /// </summary>
    public class TestWorkProvider
    {
        private readonly TestUnitCollection tests;
        private TestRunRequestsStorage requests;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestWorkProvider"/> class.
        /// </summary>
        /// <param name="tests">The tests.</param>
        /// <param name="requests">The requests.</param>
        public TestWorkProvider(TestUnitCollection tests, TestRunRequestsStorage requests)
        {
            this.tests = tests;
            this.requests = requests;
            requests.Added += OnRequestAdded;
        }

        private void OnRequestAdded(object sender, EventArgs<TestRunRequest> e)
        {
            throw new NotImplementedException();
        }
    }

	
}