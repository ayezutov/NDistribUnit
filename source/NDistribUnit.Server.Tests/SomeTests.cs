using System.IO;
using Autofac;
using NUnit.Framework;

namespace NDistribUnit.Server.Tests
{
    [TestFixture]
    public class SomeTests
    {
        [Test]
        public void MultipleRegistrationOfSameInterfaceAreAccepted()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<FileStream>().As<Stream>();
            builder.RegisterType<MemoryStream>().As<Stream>();

            var container = builder.Build();
            var stream = container.Resolve<Stream>();

            builder = new ContainerBuilder();
            builder.RegisterType<FileStream>().As<Stream>();

            builder.Update(container);

            stream = container.Resolve<Stream>();
        }
    }
}