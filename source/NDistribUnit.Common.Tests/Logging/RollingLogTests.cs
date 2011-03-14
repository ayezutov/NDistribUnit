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
            log.BeginActivity("Test execution started");
            log.Info("First step is performed");
            log.Warning("First step warning");
            log.Warning("First step warning with exception", new NotSupportedException("The action is not supported"));
            log.Error("Something bad happenned");
            log.Error("Something bad happenned with exception",
                      new NotSupportedException("The action was never and will be not supported"));
            log.Success("And finally success somewhere");
            log.EndActivity("First step is finished");

            Assert.That(log.GetEntries(0, Int32.MaxValue).Count(), Is.EqualTo(7));
        }
    }
}