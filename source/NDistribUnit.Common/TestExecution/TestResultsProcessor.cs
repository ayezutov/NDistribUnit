using System;
using System.IO;
using NDistribUnit.Common.TestExecution;
using NUnit.Core;

namespace NDistribUnit.Common.Contracts.DataContracts
{
    /// <summary>
    /// Results processor
    /// </summary>
    public class TestResultsProcessor
    {
        //
        //        public void MergeTo(TestResult destination, ReprocessingManager reprocessingManager)
        //        {
        //            if (!destination.Test.IsSuite || !Test.IsSuite)
        //                throw new NotSupportedException("Only merging of test suites into each other is supported");
        //
        //            if (destination.Test.TestType != Test.TestType)
        //                throw new NotSupportedException("Only merging of test suites of same type is supported");
        //
        //            if (!HasResults)
        //                throw new NotSupportedException("Test suite result should contain child test results");
        //
        //            UpdateNameAndFullnameIfContainsSlashes();
        //            destination.UpdateNameAndFullnameIfContainsSlashes();
        //
        //            if (FullName != destination.FullName)
        //                throw new NotSupportedException("Only merging of test suites with same name from different runs are supported");
        //
        //            MergeMachineNamesTo(destination);
        //
        //            foreach (var innerDestinationResult in destination.Results)
        //            {
        //                innerDestinationResult.UpdateNameAndFullnameIfContainsSlashes();
        //            }
        //
        //            foreach (var innerResult in Results)
        //            {
        //                innerResult.UpdateNameAndFullnameIfContainsSlashes();
        //                var destinationInnerResult = destination.Results.FirstOrDefault(r => r.FullName.Equals(innerResult.FullName, StringComparison.Ordinal));
        //
        //                if (destinationInnerResult == null)
        //                    destination.Results.Add(innerResult);
        //                else if (innerResult.Test.IsSuite)
        //                    innerResult.MergeTo(destinationInnerResult, reprocessingManager);
        //                else
        //                    innerResult.MergeTestCaseTo(destinationInnerResult, reprocessingManager);
        //            }
        //        }
        //
        //        private void MergeTestCaseTo(TestResult destinationCase, ReprocessingManager reprocessingManager)
        //        {
        //            if (destinationCase.IsSuccess)
        //                return;
        //
        //            if (IsSuccess)
        //            {
        //                CopyDataTo(destinationCase);
        //                return;
        //            }
        //
        //            if (destinationCase.Message == Message || destinationCase.StackTrace == StackTrace)
        //                return;
        //
        //            if (reprocessingManager != null)
        //            {
        //                var modeForCurrent = reprocessingManager.HasExceptionalhandlingConfigurationMatch(this);
        //                var modeForDestination = reprocessingManager.HasExceptionalhandlingConfigurationMatch(destinationCase);
        //
        //                if (modeForCurrent && !modeForDestination)
        //                    return;
        //
        //                if (!modeForCurrent && modeForDestination)
        //                {
        //                    CopyDataTo(destinationCase);
        //                    return;
        //                }
        //            }
        //        }
        //
        //        private void MergeMachineNamesTo(TestResult destination)
        //        {
        //            if (MachineNames == null)
        //                return;
        //
        //            if (destination.MachineNames == null)
        //                destination.MachineNames = MachineNames;
        //
        //            destination.MachineNames.AddRange(
        //                MachineNames.Where(machineName =>
        //                    !destination.MachineNames.Contains(machineName)));
        //        }

        /// <summary>
        /// Merges the specified result to merge.
        /// </summary>
        /// <param name="resultToMerge">The result to merge.</param>
        /// <param name="merged">The merged.</param>
        public void Merge(TestUnitResult resultToMerge, ref TestResult merged)
        {
            var other = resultToMerge.Result;

            Merge(other, merged);
        }

        private void Merge(TestResult other, TestResult merged)
        {
            TestType mergedType;
            if (!Enum.TryParse(merged.Test.TestType, out mergedType))
                mergedType = TestType.Other;

            TestType otherType;
            if (!Enum.TryParse(other.Test.TestType, out otherType))
                otherType = TestType.Other;

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
                    merged.Results.Add(otherChild);
                    continue;
                }
                Merge(otherChild, mergedChild);
            }
        }
    }
}