using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Core;
using System.Linq;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// Results processor
    /// </summary>
    public class TestResultsProcessor
    {
        /// <summary>
        /// Merges the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <param name="merged">The result.</param>
        public void Merge(TestResult other, TestResult merged)
        {
            var mergedType = GetType(merged);
            var otherType = GetType(other);

            if (mergedType == TestType.Project && otherType == TestType.Project)
            {
                if (string.IsNullOrEmpty(merged.FullName) && !string.IsNullOrEmpty(other.FullName))
                {
                    merged.Test.TestName.FullName = other.FullName;
                    merged.Test.TestName.Name = other.Name;
                }
            }

            if (mergedType != otherType)
                throw new NotSupportedException("Only merging of results with same test type are supported");

            if (!merged.IsSuccess && other.IsSuccess)
            {
                merged.Success(other.Message);
                merged.SetAgentName(other.GetAgentName());
            }

            MergeChildren(other, merged);
        }

        private static TestType GetType(TestResult result)
        {
            TestType mergedType;
            if (!Enum.TryParse(result.Test.TestType, out mergedType))
            {
                mergedType = result.Test.TestType.Equals("Test Project") 
                    ? TestType.Project 
                    : TestType.Other;
            }
            return mergedType;
        }

        private void MergeChildren(TestResult other, TestResult merged)
        {
            if (other.Results == null)
                return;

            foreach (TestResult otherChild in other.Results)
            {
                var mergedChild = merged.FindDescedant(
                    d =>
                        {
                            if (otherChild.Test.TestType == TestType.Assembly.ToString() 
                                && d.Test.TestType == TestType.Assembly.ToString())
                                return Path.GetFileName(d.FullName).Equals(Path.GetFileName(otherChild.FullName));
                            return d.FullName.Equals(otherChild.FullName);
                        });
                if (mergedChild == null)
                {
                    AddResult(merged, otherChild);
                    continue;
                }
                Merge(otherChild, mergedChild);
            }
        }

        private void AddResult(TestResult model, TestResult otherChild)
        {
            if (model.Results != null && model.Results.IsFixedSize)
            {
                var results = new ArrayList(model.Results);

                // Finding result by ReferenceEquals not to be tight to private variable name
                var resultsField = model.GetType()
                    .GetFields(BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic)
                    .Where(f => ReferenceEquals(f.GetValue(model), model.Results))
                    .FirstOrDefault();

                if (resultsField != null)
                    resultsField.SetValue(model, results);
            }
            model.AddResult(otherChild);
        }
    }
}