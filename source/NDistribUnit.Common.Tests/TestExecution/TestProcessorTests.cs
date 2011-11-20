using System.Collections;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.TestExecution;
using NUnit.Core;
using NUnit.Framework;
using System.Linq;

namespace NDistribUnit.Common.Tests.TestExecution
{
    [TestFixture]
    public class TestProcessorTests
    {
        private TestEntitiesGenerator data;
        private TestResultsProcessor processor;

        [SetUp]
        public void Init()
        {
            data = new TestEntitiesGenerator();
            processor = new TestResultsProcessor();
        }
        
        [Test]
        public void EnsureThatMergingOfSimilarHierarchiesWorks()
        {
            TestResult mergePoint = null;

            var branch1 = 
            data.CreateTestResult(TestType.Project, "D:/ndistribunit/somepath/project.nunit", 
            children:
            ()=> new[]
            {
                data.CreateTestResult(TestType.Assembly, "D:/ndistribunit/somepath/assembly1.dll",
                children:
                ()=>new[]
                {
                    data.CreateTestResult(TestType.Namespace, "NDistribUnit",
                    children: 
                    ()=>new[]
                    {
                        data.CreateTestResult(TestType.Namespace, "NDistribUnit.Namespace",
                        children: 
                        ()=>new[]
                        {
                            mergePoint = data.CreateTestResult(TestType.TestFixture, "NDistribUnit.Namespace.TestSuite",
                            children: 
                            ()=>new[]
                            {
                                data.CreateTestResult(TestType.TestMethod, "NDistribUnit.Namespace.TestSuite.TestMethod1"),
                                data.CreateTestResult(TestType.TestMethod, "NDistribUnit.Namespace.TestSuite.TestMethod2")
                            })
                        })
                    })
                })
            });


            var branch2 =
            data.CreateTestResult(TestType.Project, "D:/ndistribunit/somepath/project.nunit",
            children:
            () => new[]
            {
                data.CreateTestResult(TestType.Assembly, "D:/ndistribunit/somepath/assembly1.dll",
                children:
                ()=>new[]
                {
                    data.CreateTestResult(TestType.Namespace, "NDistribUnit",
                    children: 
                    ()=>new[]
                    {
                        data.CreateTestResult(TestType.Namespace, "NDistribUnit.Namespace",
                        children: 
                        ()=>new[]
                        {
                            data.CreateTestResult(TestType.TestFixture, "NDistribUnit.Namespace.TestSuite",
                            children: 
                            ()=>new[]
                            {
                                data.CreateTestResult(TestType.TestMethod, "NDistribUnit.Namespace.TestSuite.TestMethod3"),
                                data.CreateTestResult(TestType.TestMethod, "NDistribUnit.Namespace.TestSuite.TestMethod4")
                            })
                        })
                    })
                })
            });

            processor.Merge(new TestUnitResult(branch2), ref branch1);

            Assert.That(mergePoint.Results.Cast<TestResult>().Select(r => r.FullName).ToArray(), 
                Is.EquivalentTo(new[]
                                    {
                                        "NDistribUnit.Namespace.TestSuite.TestMethod1",
                                        "NDistribUnit.Namespace.TestSuite.TestMethod2",
                                        "NDistribUnit.Namespace.TestSuite.TestMethod3",
                                        "NDistribUnit.Namespace.TestSuite.TestMethod4"
                                    }));
        }
        
        [Test]
        public void EnsureThatMergingOfSuccessfulAndFailedTestsLeavesSuccess()
        {
            TestResult mergePoint = null;

            var branch1 = 
            data.CreateTestResult(TestType.Project, "D:/ndistribunit/somepath/project.nunit", 
            children:
            ()=> new[]
            {
                data.CreateTestResult(TestType.Assembly, "D:/ndistribunit/somepath/assembly1.dll",
                children:
                ()=>new[]
                {
                    data.CreateTestResult(TestType.Namespace, "NDistribUnit",
                    children: 
                    ()=>new[]
                    {
                        data.CreateTestResult(TestType.Namespace, "NDistribUnit.Namespace",
                        children: 
                        ()=>new[]
                        {
                            mergePoint = data.CreateTestResult(TestType.TestFixture, "NDistribUnit.Namespace.TestSuite",
                            children: 
                            ()=>new[]
                            {
                                data.CreateTestResult(TestType.TestMethod, "NDistribUnit.Namespace.TestSuite.TestMethod1", agentName: "FIRST"),
                                data.CreateTestResult(TestType.TestMethod, "NDistribUnit.Namespace.TestSuite.TestMethod2", ResultState.Failure, agentName:"FIRST")
                            })
                        })
                    })
                })
            });


            var branch2 =
            data.CreateTestResult(TestType.Project, "D:/ndistribunit/somepath/project.nunit",
            children:
            () => new[]
            {
                data.CreateTestResult(TestType.Assembly, "D:/ndistribunit/somepath/assembly1.dll",
                children:
                ()=>new[]
                {
                    data.CreateTestResult(TestType.Namespace, "NDistribUnit",
                    children: 
                    ()=>new[]
                    {
                        data.CreateTestResult(TestType.Namespace, "NDistribUnit.Namespace",
                        children: 
                        ()=>new[]
                        {
                            data.CreateTestResult(TestType.TestFixture, "NDistribUnit.Namespace.TestSuite",
                            children: 
                            ()=>new[]
                            {
                                data.CreateTestResult(TestType.TestMethod, "NDistribUnit.Namespace.TestSuite.TestMethod1", ResultState.Failure, agentName:"SECOND"),
                                data.CreateTestResult(TestType.TestMethod, "NDistribUnit.Namespace.TestSuite.TestMethod2", agentName:"SECOND")
                            })
                        })
                    })
                })
            });

            processor.Merge(new TestUnitResult(branch2), ref branch1);

            Assert.That(mergePoint.Results.Cast<TestResult>().Select(r => r.FullName).ToArray(), 
                Is.EquivalentTo(new[]
                                    {
                                        "NDistribUnit.Namespace.TestSuite.TestMethod1",
                                        "NDistribUnit.Namespace.TestSuite.TestMethod2"
                                    }));
            var firstChild = ((TestResult)mergePoint.Results[0]);
            Assert.That(firstChild.ResultState, Is.EqualTo(ResultState.Success));
            Assert.That(firstChild.GetAgentName(), Is.EqualTo("FIRST"));

            var secondChild = ((TestResult)mergePoint.Results[1]);
            Assert.That(secondChild.ResultState, Is.EqualTo(ResultState.Success));
            Assert.That(secondChild.GetAgentName(), Is.EqualTo("SECOND"));
        }

        [Test]
        public void EnsureThatMergingOfSameAssembliesAtDifferentPathsWorks()
        {
            var branch1 = 
            data.CreateTestResult(TestType.Project, "D:/ndistribunit/somepath/project.nunit", 
            children:
            ()=> new[]
            {
                data.CreateTestResult(TestType.Assembly, "D:/ndistribunit/somepath/assembly1.dll")
            });


            var branch2 =
            data.CreateTestResult(TestType.Project, "D:/ndistribunit/somepath/project.nunit",
            children:
            () => new[]
            {
                data.CreateTestResult(TestType.Assembly, "D:/ndistribunit/someotherpath/assembly1.dll")
            });

            processor.Merge(new TestUnitResult(branch2), ref branch1);

            Assert.That(branch1.Results.Count, Is.EqualTo(1));
        }
    }

    internal class TestInfoWrapper : ITest
    {
        public TestInfoWrapper()
        {
            Properties = new Hashtable();
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
        public int TestCount { get; set; }

        /// <summary>
        /// Categories available for this test
        /// </summary>
        public IList Categories { get; set; }

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
        public ITest Parent
        {
            get { return null; }
        }

        /// <summary>
        /// For a test suite, the child tests or suites
        /// Null if this is not a test suite
        /// </summary>
        public IList Tests
        {
            get { return null; }
        }

        /// <summary>
        /// Count the test cases that pass a filter. The
        /// result should match those that would execute
        /// when passing the same filter to Run.
        /// </summary>
        /// <param name="filter">The filter to apply</param>
        /// <returns>The count of test cases</returns>
        public int CountTestCases(ITestFilter filter)
        {
            throw new System.NotImplementedException();
        }
    }
}