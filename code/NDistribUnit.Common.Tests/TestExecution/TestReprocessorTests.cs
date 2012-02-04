using System;
using System.Collections.Generic;
using System.ServiceModel;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Server.AgentsTracking;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.TestExecution.Configuration;
using NDistribUnit.Common.Tests.TestExecution.Fixtures;
using NSubstitute;
using NUnit.Framework;

namespace NDistribUnit.Common.Tests.TestExecution
{
    [TestFixture]
    public class TestReprocessorTests
    {
        private TestReprocessor reprocessor;
        private ITestUnitsCollection collectionMock;
        private TestUnitsFixture unitsFixture;
        private TestResultsFixture resultsfactory;
        private TestRun testRun;

        [SetUp]
        public void Init()
        {
            testRun = new TestRun
                          {
                              Parameters = new TestRunParameters
                                               {
                                                   MaximumAgentsCount = 1000,
                                                   SpecialHandlings = new List<TestRunFailureSpecialHandling>()
                                               }
                          };
            unitsFixture = new TestUnitsFixture(testRun);
            resultsfactory = new TestResultsFixture();

            collectionMock = Substitute.For<ITestUnitsCollection>();
            
            reprocessor = new TestReprocessor(collectionMock, new ConsoleLog(), null);
        }

        [Test]
        public void SuccessfulResultIsNotReprocessed()
        {
            unitsFixture.Add("As.Nam1.Nam2.Nam3.Nam4.S1",
                             new[]
                                 {
                                     "tc1",
                                     "tc2",
                                     "tc3"
                                 });
            var unit = unitsFixture.Build()[0];
            resultsfactory.Initialize(new[] { unit });
            resultsfactory.Execute(unit, r => r.Success());

            reprocessor.AddForReprocessingIfRequired(unit, resultsfactory.Build(), new AgentMetadata(new EndpointAddress("net.tcp://test/")));

            collectionMock.DidNotReceive().Add(Arg.Any<TestUnitWithMetadata>());
        }

        [Test]
        public void IgnoredResultIsNotReprocessed()
        {
            unitsFixture.Add("As.Nam1.Nam2.Nam3.Nam4.S1",
                             new[]
                                 {
                                     "tc1",
                                     "tc2",
                                     "tc3"
                                 });
            var unit = unitsFixture.Build()[0];
            resultsfactory.Initialize(new[] { unit });
            resultsfactory.Execute(unit, r => r.Ignore((string)null));

            reprocessor.AddForReprocessingIfRequired(unit, resultsfactory.Build(), new AgentMetadata(new EndpointAddress("net.tcp://test/")));

            collectionMock.DidNotReceiveWithAnyArgs().Add(Arg.Any<TestUnitWithMetadata>());
        }

        [Test]
        public void FailedOrErroredResultIsNotReprocessedWhenNoHandlingInstructionsGiven()
        {
            unitsFixture.Add("As.Nam1.Nam2.Nam3.Nam4.S1",
                             new[]
                                 {
                                     "tc1",
                                     "tc2",
                                     "tc3"
                                 });
            unitsFixture.Add("As.Nam1.Nam2.Nam3.Nam4.S2",
                             new[]
                                 {
                                     "tc1",
                                     "tc2",
                                     "tc3"
                                 });
            var units = unitsFixture.Build();
            resultsfactory.Initialize(units);
            resultsfactory.Execute(units[0], r => r.Error(resultsfactory.GetInitilalizedException<NotImplementedException>()));
            resultsfactory.Execute(units[1], r => r.Failure("Error message", "Error stack trace"));

            var result = resultsfactory.Build();

            var agent = new AgentMetadata(new EndpointAddress("net.tcp://test/"));
            reprocessor.AddForReprocessingIfRequired(units[0], result, agent);
            reprocessor.AddForReprocessingIfRequired(units[1], result, agent);

            collectionMock.DidNotReceiveWithAnyArgs().Add(Arg.Any<TestUnitWithMetadata>());
        }

        [Test]
        public void FailedResultsIsProcessWhenMatchingInstructionsGiven()
        {
            unitsFixture.Add("As.Nam1.Nam2.Nam3.Nam4.S1",
                             new[]
                                 {
                                     "tc1",
                                     "tc2",
                                     "tc3"
                                 });
            unitsFixture.Add("As.Nam1.Nam2.Nam3.Nam4.S2",
                             new[]
                                 {
                                     "tc1",
                                     "tc2",
                                     "tc3"
                                 });
            var units = unitsFixture.Build();
            resultsfactory.Initialize(units);

            resultsfactory.Execute(units[0], r => r.Error(resultsfactory.GetInitilalizedException<NotImplementedException>()));
            resultsfactory.Execute(units[1], r => r.Failure("Unit 1 failed", "Error stack trace"));

            var result = resultsfactory.Build();

            testRun.Parameters.SpecialHandlings.Add(new TestRunFailureSpecialHandling
                                                        {
                                                            FailureMessage = "Unit 1",
                                                            FailureMessageType = MatchType.ContainsText,
                                                            RetryCount = 2
                                                        });

            testRun.Parameters.SpecialHandlings.Add(new TestRunFailureSpecialHandling
                                                        {
                                                            FailureStackTrace = resultsfactory.GetType().Namespace,
                                                            FailureStackTraceType = MatchType.ContainsText,
                                                            RetryCount = 2
                                                        });

            var agent = new AgentMetadata(new EndpointAddress("net.tcp://test/"));
            reprocessor.AddForReprocessingIfRequired(units[0], result, agent);
            reprocessor.AddForReprocessingIfRequired(units[1], result, agent);

            collectionMock
                .Received(1)
                .Add(Arg.Is<TestUnitWithMetadata>(v => v.FullName.Equals("As.Nam1.Nam2.Nam3.Nam4.S1")));
            collectionMock.Received(1)
                .Add(Arg.Is<TestUnitWithMetadata>(v => v.FullName.Equals("As.Nam1.Nam2.Nam3.Nam4.S2")));
        }



        [Test]
        public void ChildrenAreAddedAsSeparateUnits()
        {
            unitsFixture.Add("As.Nam1.Nam2.Nam3.Nam4.S1",
                             new[]
                                 {
                                     "tc1",
                                     "tc2",
                                     "tc3"
                                 });

            var units = unitsFixture.Build();
            resultsfactory.Initialize(units);

            resultsfactory.Execute(units[0].Children[0], r => r.Failure("Unit 1 failed", "Error stack trace 1"));
            resultsfactory.Execute(units[0].Children[2], r => r.Failure("Unit 3 failed", "Error stack trace 3"));

            var result = resultsfactory.Build();

            testRun.Parameters.SpecialHandlings.Add(
                new TestRunFailureSpecialHandling
                {
                    FailureMessage = "Unit 1",
                    FailureMessageType = MatchType.ContainsText,
                    RetryCount = 2
                });
            
            testRun.Parameters.SpecialHandlings.Add(
                new TestRunFailureSpecialHandling
                {
                    FailureMessage = "^U.+led$",
                    FailureMessageType = MatchType.Regex,
                    RetryCount = 2
                });

            var agent = new AgentMetadata(new EndpointAddress("net.tcp://test/"));
            reprocessor.AddForReprocessingIfRequired(units[0], result, agent);

            collectionMock.Received(1)
                .Add(Arg.Is<TestUnitWithMetadata>(v => v.FullName.Equals("As.Nam1.Nam2.Nam3.Nam4.S1.tc1")));
            collectionMock.Received(1)
                .Add(Arg.Is<TestUnitWithMetadata>(v => v.FullName.Equals("As.Nam1.Nam2.Nam3.Nam4.S1.tc3")));
        }

        [Test]
        public void ChildrenAreAddedAsSuiteIfAllFail()
        {
            unitsFixture.Add("As.Nam1.Nam2.Nam3.Nam4.S1",
                             new[]
                                 {
                                     "tc1",
                                     "tc2",
                                     "tc3"
                                 });

            var units = unitsFixture.Build();
            resultsfactory.Initialize(units);

            resultsfactory.Execute(units[0].Children[0], r => r.Failure("Unit 1 failed", "Error stack trace 1"));
            resultsfactory.Execute(units[0].Children[1], r => r.Failure("Unit 2 failed", "Error stack trace 2"));
            resultsfactory.Execute(units[0].Children[2], r => r.Failure("Unit 3 failed", "Error stack trace 3"));

            var result = resultsfactory.Build();

            testRun.Parameters.SpecialHandlings.Add(
                new TestRunFailureSpecialHandling
                {
                    FailureMessage = "Unit 1",
                    FailureMessageType = MatchType.ContainsText,
                    RetryCount = 2
                });
            
            testRun.Parameters.SpecialHandlings.Add(
                new TestRunFailureSpecialHandling
                {
                    FailureMessage = "^U.+led$",
                    FailureMessageType = MatchType.Regex,
                    RetryCount = 2
                });

            var agent = new AgentMetadata(new EndpointAddress("net.tcp://test/"));
            reprocessor.AddForReprocessingIfRequired(units[0], result, agent);


            collectionMock.Received(1)
                .Add(Arg.Is<TestUnitWithMetadata>(v => v.FullName.Equals("As.Nam1.Nam2.Nam3.Nam4.S1")));
        }

        [Test]
        public void ChildReprocessingSuccedes()
        {
            unitsFixture.Add("As.Nam1.Nam2.Nam3.Nam4.S1",
                             new[]
                                 {
                                     "tc1",
                                     "tc2",
                                     "tc3"
                                 });

            var units = unitsFixture.Build();
            resultsfactory.Initialize(units);

            resultsfactory.Execute(units[0], r => r.Failure("Suite failed", "Error for suite"));
            resultsfactory.Execute(units[0].Children[0], r => r.Failure("Unit 1 failed", "Error stack trace 1"));
            resultsfactory.Execute(units[0].Children[1], r => r.Failure("Unit 2 failed", "Error stack trace 2"));
            resultsfactory.Execute(units[0].Children[2], r => r.Failure("Unit 3 failed", "Error stack trace 3"));

            var result = resultsfactory.Build();
            
            testRun.Parameters.SpecialHandlings.Add(
                new TestRunFailureSpecialHandling
                {
                    FailureMessage = "^U.+led$",
                    FailureMessageType = MatchType.Regex,
                    RetryCount = 2
                });

            var agent = new AgentMetadata(new EndpointAddress("net.tcp://test/"));
            reprocessor.AddForReprocessingIfRequired(units[0].Children[1], result, agent);


            collectionMock.Received(1)
                .Add(Arg.Is<TestUnitWithMetadata>(v => v.FullName.Equals("As.Nam1.Nam2.Nam3.Nam4.S1.tc2")));
        }

        [Test]
        public void ChildReprocessingIsPerformedFixedAmountOfTimes()
        {
            unitsFixture.Add("As.Nam1.Nam2.Nam3.Nam4.S1",
                             new[]
                                 {
                                     "tc1",
                                     "tc2",
                                     "tc3"
                                 });

            var units = unitsFixture.Build();
            resultsfactory.Initialize(units);

            resultsfactory.Execute(units[0], r => r.Failure("Suite failed", "Error for suite"));
            resultsfactory.Execute(units[0].Children[0], r => r.Ignore("Unit 1 failed", "Error stack trace 1"));
            resultsfactory.Execute(units[0].Children[1], r => r.Success());
            resultsfactory.Execute(units[0].Children[2], r => r.Failure("Unit 3 failed", "Error stack trace 3"));

            var result = resultsfactory.Build();
            
            testRun.Parameters.SpecialHandlings.Add(
                new TestRunFailureSpecialHandling
                {
                    FailureMessage = "^U.+led$",
                    FailureMessageType = MatchType.Regex,
                    RetryCount = 2
                });

            var agent = new AgentMetadata(new EndpointAddress("net.tcp://test/"));
            reprocessor.AddForReprocessingIfRequired(units[0], result, agent);

            for (int i = 0; i < 20; i++)
            {
                reprocessor.AddForReprocessingIfRequired(units[0], result, agent);
            }


            collectionMock.Received(2)
                .Add(Arg.Is<TestUnitWithMetadata>(v => v.FullName.Equals("As.Nam1.Nam2.Nam3.Nam4.S1.tc3")));
        }

        [Test]
        public void ChildReprocessingIsPerformedFixedAmountOfTimes2()
        {
            unitsFixture.Add("As.Nam1.Nam2.Nam3.Nam4.S1",
                             new[]
                                 {
                                     "tc1",
                                     "tc2",
                                     "tc3"
                                 });

            var units = unitsFixture.Build();
            resultsfactory.Initialize(units);

            resultsfactory.Execute(units[0], r => r.Failure("Suite failed", "Error for suite"));
            resultsfactory.Execute(units[0].Children[0], r => r.Ignore("Unit 1 failed", "Error stack trace 1"));
            resultsfactory.Execute(units[0].Children[1], r => r.Success());
            resultsfactory.Execute(units[0].Children[2], r => r.Failure("Unit 3 failed", "Error stack trace 3"));

            var result = resultsfactory.Build();
            
            testRun.Parameters.SpecialHandlings.Add(
                new TestRunFailureSpecialHandling
                {
                    FailureMessage = "^U.+led$",
                    FailureMessageType = MatchType.Regex,
                    RetryCount = 2
                });

            var agent = new AgentMetadata(new EndpointAddress("net.tcp://test/"));
            reprocessor.AddForReprocessingIfRequired(units[0], result, agent);

            for (int i = 0; i < 20; i++)
            {
                reprocessor.AddForReprocessingIfRequired(units[0].Children[2], result, agent);
            }
            

            collectionMock
                .Received(2)
                .Add(Arg.Is<TestUnitWithMetadata>(v => v.FullName.Equals("As.Nam1.Nam2.Nam3.Nam4.S1.tc3")));

            collectionMock.DidNotReceive()
                .Add(Arg.Is<TestUnitWithMetadata>(v => !v.FullName.Equals("As.Nam1.Nam2.Nam3.Nam4.S1.tc3")));
        }

        [Test]
        public void ParentReprocessingIsPerformedFixedAmountOfTimes2()
        {
            unitsFixture.Add("As.Nam1.Nam2.Nam3.Nam4.S1",
                             new[]
                                 {
                                     "tc1",
                                     "tc2",
                                     "tc3"
                                 });

            var units = unitsFixture.Build();
            resultsfactory.Initialize(units);

            resultsfactory.Execute(units[0], r => r.Failure("U failed", "Error for suite"));
            resultsfactory.Execute(units[0].Children[0], r => r.Ignore("Unit 1 failed", "Error stack trace 1"));
            resultsfactory.Execute(units[0].Children[1], r => r.Success());
            resultsfactory.Execute(units[0].Children[2], r => r.Failure("Unit 3 failed", "Error stack trace 3"));

            var result = resultsfactory.Build();
            
            testRun.Parameters.SpecialHandlings.Add(
                new TestRunFailureSpecialHandling
                {
                    FailureMessage = "^U.+led$",
                    FailureMessageType = MatchType.Regex,
                    RetryCount = 2
                });

            var agent = new AgentMetadata(new EndpointAddress("net.tcp://test/"));
            reprocessor.AddForReprocessingIfRequired(units[0], result, agent);

            for (int i = 0; i < 20; i++)
            {
                reprocessor.AddForReprocessingIfRequired(units[0], result, agent);
            }

            collectionMock.Received(2)
                .Add(Arg.Is<TestUnitWithMetadata>(v => v.FullName.Equals("As.Nam1.Nam2.Nam3.Nam4.S1")));
            collectionMock.DidNotReceive()
                .Add(Arg.Is<TestUnitWithMetadata>(v => !v.FullName.Equals("As.Nam1.Nam2.Nam3.Nam4.S1")));
        }
    }
}