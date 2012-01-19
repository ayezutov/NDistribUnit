using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using NUnit.Core;

namespace NDistribUnit.Common.TestExecution
{
    internal class TestDataProvider : ITest
    {
        public TestDataProvider()
        {
            categories = new List<string>();
            Properties = new Hashtable();
        }

        public TestDataProvider(ITest test): this()
        {
            TestName = (TestName)test.TestName.Clone();
            TestType = test.TestType;

            RunState = test.RunState;
            IgnoreReason = test.IgnoreReason;
            Description = test.Description;
            IsSuite = test.IsSuite;

            if (test.Categories != null)
                categories.AddRange(test.Categories.Cast<string>());
            if (test.Properties != null)
            {
                this.Properties = new ListDictionary();
                foreach (DictionaryEntry entry in test.Properties)
                    Properties.Add(entry.Key, entry.Value);
            }

            TestCount = test.TestCount;
        }

        /// <summary>
        /// Gets the completely specified name of the test
        /// encapsulated in a TestName object.
        /// </summary>
        public TestName TestName { get; set; }

        /// <summary>
        /// Gets a string representing the type of test, e.g.: "Test Case"
        /// </summary>
        public string TestType { get; set; }

        /// <summary>
        /// Indicates whether the test can be run using
        /// the RunState enum.
        /// </summary>
        public RunState RunState { get; set; }

        /// <summary>
        /// Reason for not running the test, if applicable
        /// </summary>
        public string IgnoreReason { get; set; }

        /// <summary>
        /// Count of the test cases ( 1 if this is a test case )
        /// </summary>
        public int TestCount { get; private set; }

        private List<string> categories;

        /// <summary>
        /// Categories available for this test
        /// </summary>
        public IList Categories
        {
            get { return categories; }
            private set { categories = value.Cast<string>().ToList(); }
        }

        /// <summary>
        /// Return the description field. 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Return additional properties of the test
        /// </summary>
        public IDictionary Properties { get; private set; }

        /// <summary>
        /// True if this is a suite
        /// </summary>
        public bool IsSuite { get; set; }

        /// <summary>
        ///  Gets the parent test of this test
        /// </summary>
        public ITest Parent { get; private set; }

        /// <summary>
        /// For a test suite, the child tests or suites
        /// Null if this is not a test suite
        /// </summary>
        public IList Tests { get; private set; }

        /// <summary>
        /// Count the test cases that pass a filter. The
        /// result should match those that would execute
        /// when passing the same filter to Run.
        /// </summary>
        /// <param name="filter">The filter to apply</param>
        /// <returns>The count of test cases</returns>
        public int CountTestCases(ITestFilter filter)
        {
            if (filter.IsEmpty)
                return TestCount;

            if (!IsSuite)
                return filter.Pass(this) ? 1 : 0;

            int count = 0;
            if (filter.Pass(this))
            {
                count += Tests.Cast<ITest>().Sum(test => test.CountTestCases(filter));
            }
            return count;
        }
    }
}