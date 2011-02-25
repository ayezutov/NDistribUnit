using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace NDistribUnit.Server.Tests
{
    [TestFixture]
    public class JavascriptTests
    {
        [Test, Explicit]
        public void RunAllTests()
        {
            var process = 
                new Process
                {
                    StartInfo = new ProcessStartInfo
                                    {
                                        FileName = @"C:\Program Files\Internet Explorer\iexplore.exe",
                                        Arguments = "http://localhost:9876"
                                    }
                };
            process.Start();

            Thread.Sleep(5000);

            process.Kill();
        }
    }
}
