using System;
using NDistribUnit.Common.Logging;
using NUnit.Framework;
using System.Linq;

namespace NDistribUnit.Common.Tests.Logging
{
    [TestFixture]
    public class RollingLogTests
    {
        [Test]
        public void EnsureThatLogIsSaved()
        {
            var log = new RollingLog(20);
            AddEightItemsToLog(log);
            Assert.That(log.GetEntries(null, Int32.MaxValue).Count(), Is.EqualTo(8));
        }

        [Test]
        public void EnsureThatFetchingLessItemsWorks()
        {
            var log = new RollingLog(20);
            AddEightItemsToLog(log);
            LogEntry[] logEntries = log.GetEntries(null, 3);
            Assert.That(logEntries.Count(), Is.EqualTo(3));
            Assert.That(logEntries[0].Type, Is.EqualTo(LogEntryType.ActivityStart));
            Assert.That(logEntries[2].Type, Is.EqualTo(LogEntryType.Warning));
        }

        [Test]
        public void EnsureThatFetchingAfterOverflowIsCorrect()
        {
            var log = new RollingLog(5);
            AddEightItemsToLog(log);
            LogEntry[] logEntries = log.GetEntries(null, 3);
            Assert.That(logEntries.Count(), Is.EqualTo(3));
            Assert.That(logEntries[0].Type, Is.EqualTo(LogEntryType.Warning));
            Assert.That(logEntries[1].Type, Is.EqualTo(LogEntryType.Error));
        }

        [Test]
        public void EnsureNextItemsAreReturnedCorrectly()
        {
            var log = new RollingLog(20);
            AddEightItemsToLog(log);
            LogEntry[] logEntries = log.GetEntries(null, 3);
            Assert.That(logEntries.Count(), Is.EqualTo(3));

            logEntries = log.GetEntries(logEntries[2].Id, 3);
            Assert.That(logEntries.Count(), Is.EqualTo(3));
            Assert.That(logEntries[0].Type, Is.EqualTo(LogEntryType.Warning));
            Assert.That(logEntries[1].Type, Is.EqualTo(LogEntryType.Error));
        }


        [Test]
        public void EnsureThatFetchingNextItemsAfterOverflowIsCorrect()
        {
            var log = new RollingLog(6);
            AddEightItemsToLog(log);
            LogEntry[] logEntries = log.GetEntries(null, 3);
            Assert.That(logEntries.Count(), Is.EqualTo(3));


            logEntries = log.GetEntries(logEntries[2].Id, 3);
            Assert.That(logEntries.Count(), Is.EqualTo(3));
            Assert.That(logEntries[1].Type, Is.EqualTo(LogEntryType.Success));
        }

        private static void AddEightItemsToLog(RollingLog log)
        {
            log.BeginActivity("Test execution started");
            log.Info("First step is performed");
            log.Warning("First step warning");
            log.Warning("First step warning with exception", new NotSupportedException("The action is not supported"));
            log.Error("Something bad happenned");
            log.Error("Something bad happenned with exception", new NotSupportedException("The action was never and will be not supported"));
            log.Success("And finally success somewhere");
            log.EndActivity("First step is finished");
        }
    }
}