using NDistribUnit.Common.Client;
using NDistribUnit.Integration.Tests.Infrastructure.Stubs;

namespace NDistribUnit.Integration.Tests.Infrastructure.Entities
{
    public class ClientWrapper
    {
        /// <summary>
        /// Gets or sets the test runnner.
        /// </summary>
        /// <value>
        /// The test runnner.
        /// </value>
        public Client TestRunner { get; private set; }

        public TestUpdateReceiver UpdateReceiver { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientWrapper"/> class.
        /// </summary>
        /// <param name="testRunnner">The test runnner.</param>
        /// <param name="updateReceiver">The update receiver.</param>
        public ClientWrapper(Client testRunnner, TestUpdateReceiver updateReceiver)
        {
            TestRunner = testRunnner;
            UpdateReceiver = updateReceiver;
        }

        public void RunEmptyTest()
        {
            TestRunner.Run();
        }
    }
}