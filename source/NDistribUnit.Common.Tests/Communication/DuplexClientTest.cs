using System;
using System.ServiceModel;
using NDistribUnit.Common.Communication;
using NUnit.Framework;

namespace NDistribUnit.Common.Tests.Communication
{
    [TestFixture]
    public class DuplexClientTest
    {
        [SetUp]
        public void Init()
        {
            
        }

        [TearDown]
        public void CleanUp()
        {
        }

        [Test]
        public void TestThatClientIsPingingTheServer()
        {
            var url = "http://localhost/" + Guid.NewGuid();

            var host = new ServiceHost(typeof (TestService));
            host.AddServiceEndpoint(typeof (ITestInterface), new WSDualHttpBinding(),
                                    url);
            host.Open();

            using (host)
            {
                var client = new DuplexClient<ITestInterface>(new TestCallbackObject(), new WSDualHttpBinding(),
                                                              new EndpointAddress(url));
                client.Open();
                try
                {
                    client.Actions.PerformAction();
                }
                finally
                {
                    client.Close();
                }
            }

        }
    }

    [ServiceContract(CallbackContract = typeof(ITestCallbackContract))]
    internal interface ITestInterface : IDuplexService
    {
        [OperationContract]
        void PerformAction();
    }

    internal interface ITestCallbackContract
    {
        [OperationContract]
        int Notify();
    }

    class TestCallbackObject : ITestCallbackContract
    {
        public int Notify()
        {
            throw new NotImplementedException();
        }
    }

    class TestService : ITestInterface
    {
        public void PerformAction()
        {
        }

        public void PerformHeartbeat()
        {
        }
    }
}