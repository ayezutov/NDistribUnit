using System;
using System.Collections.Generic;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.TestExecution;
using NUnit.Core;
using System.Linq;

namespace NDistribUnit.Common.Tests.TestExecution.Fixtures
{
    /// <summary>
    /// Creates test units
    /// </summary>
    public class TestUnitsFixture
    {
        private readonly TestRun testRun;
        private readonly List<TestUnitWithMetadata> data = new List<TestUnitWithMetadata>();
        private string assemblyName;

        public TestUnitsFixture(TestRun testRun)
        {
            this.testRun = testRun;
        }

        public TestUnitWithMetadata[] Build()
        {
            return data.ToArray();
        }

        public void SetAssembly(string name)
        {
            assemblyName = name;
        }

        public void Add(string suiteName, IEnumerable<string> casesNames)
        {
            data.Add(new TestUnitWithMetadata(
                    testRun, 
                    new TestDataProvider
                        {
                            TestName = new TestName
                                        {
                                            FullName = suiteName,
                                            Name = suiteName.Split('.').LastOrDefault() ?? suiteName
                                        },
                            IsSuite = true,
                            TestType = TestType.TestFixture.ToString()
                        }, 
                    assemblyName ?? (assemblyName = (Guid.NewGuid() + ".dll")),
                    casesNames.Select(c=> new TestUnitWithMetadata(
                                              testRun,
                                              new TestDataProvider
                                                  {
                                                      TestName = new TestName
                                                                     {
                                                                         FullName = string.Format("{0}.{1}", suiteName, c),
                                                                         Name = c
                                                                     },
                                                      IsSuite = false,
                                                      TestType = TestType.TestMethod.ToString()
                                                  },
                                              assemblyName)).ToList()
                ));
        }
    }
}