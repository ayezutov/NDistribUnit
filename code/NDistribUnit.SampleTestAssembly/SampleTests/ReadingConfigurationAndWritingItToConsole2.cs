using System;
using System.Configuration;
using NUnit.Framework;

namespace NDistribUnit.SampleTestAssembly.SampleTests
{
    [TestFixture]
    public class ReadingConfigurationAndWritingItToConsole2
    {
        [Test]
        public void Test1()
        {
            ReadingConfigurationAndWritingItToConsole.RunSample();
        }
        [Test]
        public void Test2()
        {
            ReadingConfigurationAndWritingItToConsole.RunSample();
        }
        [Test]
        public void Test3()
        {
            ReadingConfigurationAndWritingItToConsole.RunSample();
        }
    }
}