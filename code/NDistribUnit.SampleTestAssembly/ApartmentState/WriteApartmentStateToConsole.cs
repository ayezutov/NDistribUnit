using System;
using System.Threading;
using NUnit.Framework;

namespace NDistribUnit.SampleTestAssembly.ApartmentState
{
    [TestFixture]
    public class WriteApartmentStateToConsole
    {
        [Test]
        public void Test()
        {
            Console.WriteLine(Thread.CurrentThread.GetApartmentState());
        }
    }
}