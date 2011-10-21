using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Moq;
using NDistribUnit.Common.Common.Logging;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Logging;
using NUnit.Framework;

namespace NDistribUnit.Integration.Tests.TestExecution
{
    [TestFixture]
    public class TemporaryTestForPlaying
    {
        [SetUp]
        public void Init()
        {
        }

        [TearDown]
        public void Dispose()
        {
        }

        [TestFixtureSetUp]
        public void InitOnce()
        {
        }

        [TestFixtureTearDown]
        public void DisposeOnce()
        {
        }

        [Test]
        public void CanBuildMultipleContainers()
        {
            var b = new ContainerBuilder();

            b.RegisterType<ConsoleLog>().As<ILog>();

            IContainer container = b.Build();

            Assert.That(container.Resolve<ILog>(), Is.InstanceOf<ConsoleLog>());

            var b2 = new ContainerBuilder();
            b2.Register(c => new RollingLog(1000)).As<ILog>();
            b2.Update(container);
            
            Assert.That(container.Resolve<ILog>(), Is.InstanceOf<RollingLog>());

            var logs = container.Resolve<IEnumerable<ILog>>();

            Assert.That(logs.Count(), Is.EqualTo(2));
        }

        [Test]
        public void OptionalParametersCanBeIgnored()
        {
            var b = new ContainerBuilder();

            b.RegisterType<ConsoleLog>().As<ILog>().AsSelf();
            b.RegisterType<OptionalParamsInConstructor>();

            IContainer container = b.Build();

            Assert.That(container.Resolve<OptionalParamsInConstructor>().RollingLog, Is.Null);
        }

        [Test]
        public void AnotherMockWorks()
        {
            var builder = new ContainerBuilder();

            var vp = new Mock<IVersionProvider>();
            vp.Setup(v => v.GetVersion()).Returns(new Version("1.0.0.0"));

            builder.RegisterInstance(vp.Object).As<IVersionProvider>();

            var container = builder.Build();

            var versionProvider1 = container.Resolve<IVersionProvider>();

            var builder2 = new ContainerBuilder();

            var vp2 = new Mock<IVersionProvider>();
            vp2.Setup(v => v.GetVersion()).Returns(new Version("2.0.0.0"));

            builder2.RegisterInstance(vp2.Object).As<IVersionProvider>();

            builder2.Update(container);

            var versionProvider2 = container.Resolve<IVersionProvider>();

            Assert.That(versionProvider2.GetVersion(), Is.EqualTo(new Version("2.0.0.0")));
            Assert.That(versionProvider1.GetVersion(), Is.EqualTo(new Version("1.0.0.0")));
        }


        private class OptionalParamsInConstructor
        {
            public ConsoleLog ConsoleLog { get; set; }
            public RollingLog RollingLog { get; set; }

            public OptionalParamsInConstructor(ConsoleLog consoleLog, RollingLog rollingLog = null)
            {
                ConsoleLog = consoleLog;
                RollingLog = rollingLog;
            }
        }
    }
}