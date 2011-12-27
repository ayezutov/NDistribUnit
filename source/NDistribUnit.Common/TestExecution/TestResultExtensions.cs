﻿using System;
using System.Linq;
using System.Reflection;
using NUnit.Core;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// A class with extensions for convenient access to NDistribUnit properties
    /// </summary>
    public static class TestResultExtensions
    {
        private const string AgentNameProperty = "ndistribunit.agent-name";

        /// <summary>
        /// Sets the name of the machine.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static void SetAgentName(this TestResult result, string name)
        {
            result.Test.Properties[AgentNameProperty] = name;
        }

        /// <summary>
        /// Gets the name of the machine.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public static string GetAgentName(this TestResult result)
        {
            return (string) (result.Test.Properties.Contains(AgentNameProperty) ? result.Test.Properties[AgentNameProperty] : null);
        }

        /// <summary>
        /// Sets the type of the test.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="type">The type.</param>
        public static void SetTestType(this TestResult result, string type)
        {
            var test = (result.Test as TestInfo);
            if (test == null) return;

            var field = result.Test.GetType().GetField("testType", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(test, type);
        }

        /// <summary>
        /// Foreaches the specified result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="action">The action.</param>
        public static void ForSelfAndAllDescedants(this TestResult result, Action<TestResult> action)
        {
            action(result);
            if (result.Results != null)
                foreach (TestResult child in result.Results)
                {
                    ForSelfAndAllDescedants(child, action);
                }
        }

        /// <summary>
        /// Finds the descedant.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="condition">The condition.</param>
        /// <returns></returns>
        public static TestResult FindDescedant(this TestResult result, Func<TestResult, bool> condition)
        {
            if (result.Results == null)
                return null;

            var foundChild = result.Results.Cast<TestResult>().Where(condition).FirstOrDefault();
            if (foundChild != null)
                return foundChild;

            return (from TestResult child 
                        in result.Results 
                        select child.FindDescedant(condition))
                    .FirstOrDefault(foundDescedant => foundDescedant != null);
        }
    }
}