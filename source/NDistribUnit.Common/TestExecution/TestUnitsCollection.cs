using System;
using System.Collections.Generic;
using NDistribUnit.Common.Extensions;

namespace NDistribUnit.Common.TestExecution
{
	/// <summary>
	/// 
	/// </summary>
	public class TestUnitsCollection
	{
        /// <summary>
        /// The synchronization object, which is used for thread safe access to that collection
        /// </summary>
	    public readonly object SyncObject = new object();
        private readonly List<TestUnit> available = new List<TestUnit>();
        private readonly List<TestUnit> running = new List<TestUnit>();

        /// <summary>
        /// Occurs when a test unit is added to available for running collection
        /// </summary>
	    public EventHandler<EventArgs<TestUnit>> AvailableAdded;

        /// <summary>
        /// Occurs when a test unit is moved to running collection
        /// </summary>
	    public EventHandler<EventArgs<TestUnit>> RunningAdded;

	    /// <summary>
        /// Adds the range of test units into the collection.
        /// </summary>
        /// <param name="testUnits">The test units.</param>
	    public void AddRange(IEnumerable<TestUnit> testUnits)
	    {
            foreach (var testUnit in testUnits)
            {
                Add(testUnit);
            }
	    }

        /// <summary>
        /// Adds the specified test unit.
        /// </summary>
        /// <param name="testUnit">The test unit.</param>
	    public void Add(TestUnit testUnit)
	    {
	        lock (SyncObject)
	        {
	            available.Add(testUnit);
	        }
            AvailableAdded.SafeInvoke(this, testUnit);
	    }

        /// <summary>
        /// Moves to running.
        /// </summary>
        /// <param name="testUnit">The test unit.</param>
        public void MarkRunning(TestUnit testUnit)
        {
            lock (SyncObject)
            {
                if (running.Contains(testUnit))
                    return;

                available.Remove(testUnit);
                running.Add(testUnit);
            }
            RunningAdded.SafeInvoke(this, testUnit);
        }

        /// <summary>
        /// Marks the completed.
        /// </summary>
        /// <param name="test">The test.</param>
	    public void MarkCompleted(TestUnit test)
	    {}

        /// <summary>
        /// Gets the available.
        /// </summary>
	    public List<TestUnit> GetAvailable()
	    {
	        return available;
	    }
	}
}