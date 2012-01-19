using System;
using System.ServiceModel;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.TestExecution.DistributedConfiguration;
using NUnit.Core;

namespace NDistribUnit.Integration.Tests.Infrastructure.Stubs
{
    public class CommunicationExceptionThrowingAgent: IRemoteAppPart, IAgent
    {
        /// <summary>
        /// Pings the tracking side
        /// </summary>
        /// <param name="pingInterval"></param>
        /// <returns>Anything (including null) if everything is ok, throws exception otherwise</returns>
        public PingResult Ping(TimeSpan pingInterval)
        {
            throw new CommunicationException();
        }

        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <param name="maxItemsCount">The max items count.</param>
        /// <param name="lastFetchedEntryId">The last fetched entry id.</param>
        /// <returns></returns>
        public LogEntry[] GetLog(int maxItemsCount, int? lastFetchedEntryId)
        {
            throw new CommunicationException();
        }

        /// <summary>
        /// Receives the update pakage.
        /// </summary>
        /// <param name="updatePackage"></param>
        public void ReceiveUpdatePackage(UpdatePackage updatePackage)
        {
            throw new CommunicationException();
        }

        /// <summary>
        /// Runs tests on agent
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="configurationSubstitutions">The configuration substitutions.</param>
        /// <returns></returns>
        public TestResult RunTests(TestUnit test, DistributedConfigurationSubstitutions configurationSubstitutions)
        {
            throw new CommunicationException();
        }

        public void ReceiveProject(ProjectMessage project)
        {
            throw new CommunicationException();
        }

        public bool HasProject(TestRun run)
        {
            throw new CommunicationException();
        }
    }
}