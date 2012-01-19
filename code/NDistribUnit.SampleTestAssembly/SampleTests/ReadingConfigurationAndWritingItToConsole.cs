using System;
using System.Configuration;
using NUnit.Framework;

namespace NDistribUnit.SampleTestAssembly.SampleTests
{
    [TestFixture]
    public class ReadingConfigurationAndWritingItToConsole
    {
        [Test]
        public void Test1()
        {
            RunSample();
        }
        [Test]
        public void Test2()
        {
            RunSample();
        }
        [Test]
        public void Test3()
        {
            RunSample();
        }

        internal static void RunSample()
        {
            var sample = ConfigurationManager.AppSettings["sample"];

            Console.WriteLine(sample);

            Assert.That(sample, Is.EqualTo("sample"));
        }
    }
}