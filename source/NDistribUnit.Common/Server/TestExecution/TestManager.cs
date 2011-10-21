using System;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Server.Communication;

namespace NDistribUnit.Common.Server.TestExecution
{
	/// <summary>
	/// 
	/// </summary>
	public class TestManager
	{
		private readonly ServerConnectionsTracker agents;
		private readonly TestUnitsCollection tests;
		/// <summary>
		/// 
		/// </summary>
		public readonly object SyncRoot = new object();
		/// <summary>
		/// Initializes a new instance of the <see cref="TestManager"/> class.
		/// </summary>
		/// <param name="agents">The agents.</param>
		/// <param name="tests">The tests.</param>
		public TestManager(ServerConnectionsTracker agents, TestUnitsCollection tests)
		{
			this.agents = agents;
			agents.AgentStateChanged += OnAgentStateChanged;
			this.tests = tests;
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


	
}