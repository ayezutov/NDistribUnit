using System;
using System.Collections.Generic;
using NUnit.Core;
using NUnit.Core.Filters;
using NUnit.Util;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class NUnitTestsFilter : ITestFilter
    {
        private readonly TestFilter nativeFilter;

        /// <summary>
        /// Initializes a new instance of the <see cref="NUnitTestsFilter"/> class.
        /// </summary>
        /// <param name="include">The include parameter.</param>
        /// <param name="exclude">The exclude parameter.</param>
        /// <param name="run">The run parameter.</param>
        public NUnitTestsFilter(string include, string exclude, string run = null)
        {
            var filters = new List<TestFilter>();
            
            if (!string.IsNullOrEmpty(run))
                filters.Add(new SimpleNameFilter(run));

            if (!string.IsNullOrEmpty(include))
                filters.Add(new CategoryExpression(include).Filter);

            if (!string.IsNullOrEmpty(exclude))
                filters.Add(new NotFilter(new CategoryExpression(exclude).Filter));

            if (filters.Count == 0)
                nativeFilter = TestFilter.Empty;
            else if (filters.Count == 1)
                nativeFilter = filters[0];
            else
                nativeFilter = new AndFilter(filters.ToArray());

            var notFilter = nativeFilter as NotFilter;
            if (notFilter != null)
                notFilter.TopLevel = true;
        }

        /// <summary>
        /// Passes the specified test.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <returns></returns>
        public bool Pass(ITest test)
        {
            return nativeFilter.Pass(test);
        }

        /// <summary>
        /// Matches the specified test.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <returns></returns>
        public bool Match(ITest test)
        {
            return nativeFilter.Match(test);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty
        {
            get { return nativeFilter.IsEmpty; }
        }

        /// <summary>
        /// Gets the native filter.
        /// </summary>
        public TestFilter NativeFilter
        {
            get { return nativeFilter; }
        }
    }
}