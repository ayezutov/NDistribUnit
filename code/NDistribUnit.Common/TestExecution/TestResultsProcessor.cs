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
        /// <param name="source"> </param>
        /// <param name="target"> </param>
        public void Merge(TestResult source, TestResult target)
        {
            var mergedType = GetType(target);
            var otherType = GetType(source);

            if (mergedType == TestType.Project && otherType == TestType.Project)
            {
                if (string.IsNullOrEmpty(target.FullName) && !string.IsNullOrEmpty(source.FullName))
                {
                    target.Test.TestName.FullName = source.FullName;
                    target.Test.TestName.Name = source.Name;
                }
            }

            if (mergedType != otherType)
                throw new NotSupportedException("Only merging of results with same test type are supported");

            if (!target.IsSuccess && source.IsSuccess)
            {
                target.Success(source.Message);
                target.SetAgentName(source.GetAgentName());
            }

            MergeChildren(source, target);
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
                    .FirstOrDefault(f => ReferenceEquals(f.GetValue(model), model.Results));

                if (resultsField != null)
                    resultsField.SetValue(model, results);
            }
            model.AddResult(otherChild);
        }
    }
}