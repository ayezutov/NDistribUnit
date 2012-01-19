using System;
using NDistribUnit.Bootstrapper;
using NUnit.Framework;

namespace NDistribUnit.Client.Tests.Configuration
{
    [TestFixture]
    public class ConfigurationFileMergerTemporaryTests
    {
        [Test, Explicit]
        public void CheckOnExistingFilesOnCurrentComputer()
        {
            var @base = @"d:\work\personal\NDistribUnit\source\builds\Debug\Fixed.Version\Server\NDistribUnit.Server.exe.config";
            var part = @"d:\work\personal\NDistribUnit\source\builds\Debug\Server.exe.config";

            var merged = new ConfigurationFileMerger().MergeFilesToString(@base, part);

            Console.WriteLine(merged);
        }
    }
}