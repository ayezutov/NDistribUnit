using System;
using System.Collections.Generic;
using NDistribUnit.Common.TestExecution;
using NUnit.Core;
using System.Linq;

namespace NDistribUnit.Common.Tests.TestExecution.Fixtures
{
    public class TestResultsFixture
    {
        TestResult result;

        public void Initialize(IEnumerable<TestUnitWithMetadata> units)
        {
            var processor = new TestResultsProcessor();
            foreach (var unit in units)
            {
                var intermediateResult = TestResultFactory.GetResultForTest(unit.Test, null);
                var bl = intermediateResult.FindBottomLevelTestSuites().First();
                foreach (var child in unit.Children)
                {
                    bl.AddResult(TestResultFactory.GetResult(null, null, TestType.TestMethod.ToString(),
                                                child.Test.Info.TestName.FullName, false));
                }

                if (result == null)
                    result = intermediateResult;
                else 
                    processor.Merge(intermediateResult, result);
            }
        }
        
        public TestResult Build()
        {
            return result;
        }

        public void Execute(TestUnitWithMetadata unit, Action<TestResult> func)
        {
            var desc = unit == null ? result : result.FindDescedant(r => r.Name == unit.Test.Info.TestName.FullName);
            if (desc != null)
                func(desc);
        }

        public TException GetInitilalizedException<TException>() where TException: Exception, new()
        {
            try
            {
                throw new TException();
            }
            catch (TException ex)
            {
                return ex;
            }
        }
    }
}