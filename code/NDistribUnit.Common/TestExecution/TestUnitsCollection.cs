using System;
using System.Collections.Generic;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Extensions;
using System.Linq;

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
        private readonly List<TestUnitWithMetadata> available = new List<TestUnitWithMetadata>();
        private readonly List<TestUnitWithMetadata> running = new List<TestUnitWithMetadata>();

        /// <summary>
        /// Occurs when a test unit is added to available for running collection
        /// </summary>
	    public event EventHandler AvailableAdded;

        /// <summary>
        /// Gets a value indicating whether this instance has available.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has available; otherwise, <c>false</c>.
        /// </value>
	    public bool HasAvailable
	    {
            get { return available.Count > 0; }
	    }

	    /// <summary>
        /// Adds the range of test units into the collection.
        /// </summary>
        /// <param name="testUnits">The test units.</param>
        public void AddRange(IEnumerable<TestUnitWithMetadata> testUnits)
	    {
            lock (SyncObject)
            {
                available.AddRange(testUnits);
            }
            AvailableAdded.SafeInvoke(this);
	    }

        /// <summary>
        /// Adds the specified test unit.
        /// </summary>
        /// <param name="testUnit">The test unit.</param>
        public void Add(TestUnitWithMetadata testUnit)
	    {
	        lock (SyncObject)
	        {
	            available.Add(testUnit);
	        }
            AvailableAdded.SafeInvoke(this);
	    }

        /// <summary>
        /// Moves to running.
        /// </summary>
        /// <param name="testUnit">The test unit.</param>
        public void MarkRunning(TestUnitWithMetadata testUnit)
        {
            lock (SyncObject)
            {
                if (running.Contains(testUnit))
                    return;

                available.Remove(testUnit);
                running.Add(testUnit);
            }
        }

        /// <summary>
        /// Marks the completed.
        /// </summary>
        /// <param name="test">The test.</param>
        public void MarkCompleted(TestUnitWithMetadata test)
        {
            lock (SyncObject)
            {
                available.Remove(test);
                running.Remove(test);
            }
        }

        /// <summary>
        /// Gets the available.
        /// </summary>
        public List<TestUnitWithMetadata> GetAvailable()
	    {
	        return available;
	    }

        /// <summary>
        /// Determines whether [is any available for] [the specified test run].
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <returns>
        ///   <c>true</c> if [is any available for] [the specified test run]; otherwise, <c>false</c>.
        /// </returns>
	    public bool IsAnyAvailableFor(TestRun testRun)
	    {
            Func<TestUnitWithMetadata, bool> belongsToSameTestRun = t => t.Test.Run.Id.Equals(testRun.Id);
            return available.Any(belongsToSameTestRun) || running.Any(belongsToSameTestRun);
	    }
	}
}