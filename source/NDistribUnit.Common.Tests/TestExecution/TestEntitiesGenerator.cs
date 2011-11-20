using System;
using System.Collections;
using System.Collections.Generic;
using AYezutov.Test.Foundation;
using NDistribUnit.Common.TestExecution;
using NUnit.Core;

namespace NDistribUnit.Common.Tests.TestExecution
{
    public class TestEntitiesGenerator
    {
        public TestResult CreateTestResult(TestType type, string fullName,
                                           ResultState state = ResultState.Success, 
                                           Func<IEnumerable<TestResult>> children = null, 
                                           string stackTrace = null, 
                                           string description = null,
                                           IList categories = null,
                                           string agentName = null)
        {
            description = description ?? RandomValuesGenerator.GetRandomValue<string>();
            agentName = agentName ?? RandomValuesGenerator.GetRandomValue<string>();
            var splitted = (fullName ?? string.Empty).Split(new[]{'.'}, StringSplitOptions.RemoveEmptyEntries);

            var childResults = children != null ? children() : new TestResult[0];
            var testResult = new TestResult(new TestInfoWrapper
                                                {
                                                    TestName = new TestName
                                                                   {
                                                                       FullName = fullName,
                                                                       Name = splitted.Length > 0 ? splitted[splitted.Length - 1] : string.Empty
                                                                   },
                                                    Categories = categories,
                                                    IsSuite = type != TestType.TestMethod,
                                                    TestCount = type == TestType.TestMethod ? 1 : RandomValuesGenerator.GetRandomValue<int>(),
                                                    TestType = type.ToString()
                                                })
                                 {
                                     AssertCount = 1,
                                     Time = RandomValuesGenerator.GetRandomValue<double>()
                                 };
            if (state != ResultState.Success)
                testResult.SetResult(state, 
                                     description,
                                     stackTrace);
            else
                testResult.Success(description);

            testResult.SetAgentName(agentName);

            foreach (var childResult in childResults)
            {
                testResult.AddResult(childResult);
            }

            return testResult;
        }
    }
}