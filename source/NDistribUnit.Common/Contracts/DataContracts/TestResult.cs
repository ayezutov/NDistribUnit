using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Core;

namespace NDistribUnit.Common.Contracts.DataContracts
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class TestResult
    {
        /// <summary>
        /// Gets or sets the assert count.
        /// </summary>
        /// <value>
        /// The assert count.
        /// </value>
        public int AssertCount { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Indicates whether the test executed
        /// </summary>
        public bool Executed
        {
            get
            {
                return ResultState == ResultState.Success ||
                       ResultState == ResultState.Failure ||
                       ResultState == ResultState.Error ||
                       ResultState == ResultState.Inconclusive;
            }
        }

        /// <summary>
        /// Sets the failure site.
        /// </summary>
        /// <value>
        /// The failure site.
        /// </value>
        public FailureSite FailureSite { private get; set; }

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>
        /// The full name.
        /// </value>
        public string FullName { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance has results.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has results; otherwise, <c>false</c>.
        /// </value>
        public bool HasResults
        {
            get { return Results != null && Results.Count > 0; }
        }

        /// <summary>
        /// Sets the state of the result.
        /// </summary>
        /// <value>
        /// The state of the result.
        /// </value>
        public ResultState ResultState { private get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is error.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is error; otherwise, <c>false</c>.
        /// </value>
        public bool IsError
        {
            get { return ResultState == ResultState.Error; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is failure.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is failure; otherwise, <c>false</c>.
        /// </value>
        public bool IsFailure
        {
            get { return ResultState == ResultState.Failure; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is success.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is success; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuccess
        {
            get { return ResultState == ResultState.Success; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is ignored.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is ignored; otherwise, <c>false</c>.
        /// </value>
        public bool IsIgnored
        {
            get { return ResultState == ResultState.Ignored; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is skipped.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is skipped; otherwise, <c>false</c>.
        /// </value>
        public bool IsSkipped
        {
            get { return ResultState == ResultState.Skipped; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is invalid.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is invalid; otherwise, <c>false</c>.
        /// </value>
        public bool IsInvalid
        {
            get { return ResultState == ResultState.Inconclusive; }
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the results.
        /// </summary>
        /// <value>
        /// The results.
        /// </value>
        public List<TestResult> Results { get; set; }

        /// <summary>
        /// Gets or sets the machine names.
        /// </summary>
        /// <value>
        /// The machine names.
        /// </value>
        public List<string> MachineNames { get; set; }

        /// <summary>
        /// Gets or sets the stack trace.
        /// </summary>
        /// <value>
        /// The stack trace.
        /// </value>
        public string StackTrace { get; set; }

        /// <summary>
        /// Gets or sets the time.
        /// </summary>
        /// <value>
        /// The time.
        /// </value>
        public double Time { get; set; }

        /// <summary>
        /// Gets the failures count.
        /// </summary>
        public int FailuresCount { get; private set; }

        /// <summary>
        /// Gets the not run count.
        /// </summary>
        public int NotRunCount { get; private set; }

        /// <summary>
        /// Gets the errors count.
        /// </summary>
        public int ErrorsCount { get; private set; }

        /// <summary>
        /// Gets the ignored count.
        /// </summary>
        public int IgnoredCount { get; private set; }

        /// <summary>
        /// Gets the skipped count.
        /// </summary>
        public int SkippedCount { get; internal set; }

        /// <summary>
        /// Gets the invalid count.
        /// </summary>
        public int InvalidCount { get; internal set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public TestResult Clone()
        {
            var testResult = new TestResult();
            CopyDataTo(testResult);
            return testResult;
        }

        private void CopyDataTo(TestResult testResult)
        {
            testResult.FailuresCount = FailuresCount;
            testResult.FailureSite = FailureSite;
            testResult.FullName = FullName;
            testResult.IgnoredCount = IgnoredCount;
            testResult.InvalidCount = InvalidCount;
            testResult.MachineNames = new List<string>(MachineNames);
            testResult.Message = Message;
            testResult.Name = Name;
            testResult.NotRunCount = NotRunCount;
            testResult.Results = Results == null
                                    ? null
                                    : new List<TestResult>(Results.Select(r => r.Clone()));
            testResult.ResultState = ResultState;
            testResult.SkippedCount = SkippedCount;
            testResult.StackTrace = StackTrace;
            testResult.Time = Time;
            testResult.ErrorsCount = ErrorsCount;
            testResult.Description = Description;
            testResult.AssertCount = AssertCount;
        }

        /// <summary>
        /// Updates the tree.
        /// </summary>
        public void UpdateTree()
        {
            if (!HasResults)
            {
                NotRunCount = Executed ? 0 : 1;
                AssertCount = AssertCount;
                ErrorsCount = IsError ? 1 : 0;
                FailuresCount = IsFailure ? 1 : 0;
                IgnoredCount = IsIgnored ? 1 : 0;
                InvalidCount = IsInvalid ? 1 : 0;
                SkippedCount = IsSkipped ? 1 : 0;
                return;
            }

            foreach (var result in Results)
            {
                result.UpdateTree();
            }

            NotRunCount = Results.Sum(r => r.NotRunCount);
            AssertCount = Results.Sum(r => r.AssertCount);
            ErrorsCount = Results.Sum(r => r.ErrorsCount);
            FailuresCount = Results.Sum(r => r.FailuresCount);
            IgnoredCount = Results.Sum(r => r.IgnoredCount);
            InvalidCount = Results.Sum(r => r.InvalidCount);
            SkippedCount = Results.Sum(r => r.SkippedCount);
            if (Results[0].HasResults)
            {
                Time = Results.Sum(r => r.Time);
            }

            if (!IsSuccess && !IsIgnored)
            {
                if (Results.All(r => r.IsSuccess || r.IsIgnored || r.IsSkipped))
                {
                    Message = string.Empty;
                    StackTrace = string.Empty;
                    if (Results.All(r => r.IsIgnored))
                        ResultState = ResultState.Ignored;
                    else if (Results.All(r => r.IsSkipped))
                        ResultState = ResultState.Skipped;
                    else if (Results.All(r => r.IsIgnored || r.IsSkipped))
                        ResultState = ResultState.Ignored;
                    else
                        ResultState = ResultState.Success;
                }
                else if (!Results[0].HasResults)
                {
                    // this is innermost test suite
                    if ((string.IsNullOrEmpty(Message) || !Message.StartsWith("Child test failed")) &&
                        !Results.Any(r =>
                            !string.IsNullOrEmpty(r.Message)
                            && r.Message.StartsWith("TestFixtureSetUp failed")))
                    {
                        Message = "Child test failed";
                        StackTrace = string.Empty;
                    }
                }
            }
        }

        private void UpdateNameAndFullnameIfContainsSlashes()
        {
            if (FullName.Contains("/") || FullName.Contains("\\"))
                FullName = Name = Path.GetFileName(FullName);
        }
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
        /// Sorts the recursively.
        /// </summary>
        public void SortRecursively()
        {
            if (HasResults)
            {
                foreach (var result in Results)
                {
                    result.SortRecursively();
                }

                Results.Sort((x, y) => x.FullName.CompareTo(y.FullName));
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (IsSkipped)
                return GetWithMessage("Skipped");

            if (IsIgnored)
                return GetWithMessage("Ignored");

            if (IsInvalid)
                return GetWithMessage("Invalid");

            if (!Executed)
                return GetWithMessage("Not executed");

            if (IsSuccess)
                return "Success";

            if (IsError)
                return GetWithMessage("Error");

            if (IsFailure)
                return GetWithMessage("Failure");

            return GetWithMessage(ResultState.ToString());
        }

        private string GetWithMessage(string status)
        {
            return string.Format("{0}{1}{2}", status, string.IsNullOrEmpty(Message) ? string.Empty : ": ", Message);
        }
    }
}