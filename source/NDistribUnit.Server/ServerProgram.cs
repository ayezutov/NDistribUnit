using System;
using System.ServiceModel;

namespace NDistribUnit.Server
{
    class ServerProgram
    {
        static void Main(string[] args)
        {
            var host = new ServiceHost(typeof(TestRunner));
            host.Open();

            Console.WriteLine("Host was started");
            Console.ReadLine();
        }
    }
}
