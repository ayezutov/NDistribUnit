using System;
using NDistribUnit.Bootstrapper;
using NUnit.Framework;

namespace NDistribUnit.Client.Tests.Configuration
{
    [TestFixture]
    public class ConfigurationFileMergerTemporaryTests
    {
        [Test]
        public void CheckOnExistingFilesOnCurrentComputer()
        {
            var @base = @"d:\temp\NdistribUnit\Server\NDistribUnit.Server.exe.config";
            var part = @"d:\work\personal\NDistribUnit\source\builds\Debug\Server.exe.config";

            var merged = new InAnotherDomainConfigurationMerger().Merge(@base, part);

            Console.WriteLine(merged);
        }
    }
}