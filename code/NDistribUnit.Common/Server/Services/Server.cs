using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using NDistribUnit.Common.Common.Updating;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Contracts.ServiceContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.TestExecution;
using NDistribUnit.Common.TestExecution.Data;
using NDistribUnit.Common.TestExecution.Storage;
using NDistribUnit.Common.Updating;
using NUnit.Core;

namespace NDistribUnit.Common.Server.Services
{
    /// <summary>
    /// The test runner service, which is used by clients to tell the job
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, 
        InstanceContextMode = InstanceContextMode.Single,
        MaxItemsInObjectGraph = 2147483647)]
    public class Server : IServer
    {
        
        private readonly IUpdateSource updateSource;
        private readonly IVersionProvider versionProvider;
        private readonly IRequestsStorage requests;
        private readonly IResultsStorage resultsStorage;
        private readonly IProjectsStorage projects;
        private readonly ILog log;

        // should just hold the reference to the runner
        // so it do not gets garbage collected
        private readonly ServerTestRunner runner;

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="updateSource">The update source.</param>
        /// <param name="versionProvider">The version provider.</param>
        /// <param name="runner">The manager.</param>
        /// <param name="requests">The storage.</param>
        /// <param name="resultsStorage">The results.</param>
        /// <param name="log">The log.</param>
        /// <param name="projects">The projects.</param>
        public Server(
            IUpdateSource updateSource, 
            IVersionProvider versionProvider,
            ServerTestRunner runner,
            IRequestsStorage requests,
            IResultsStorage resultsStorage,
            ILog log, 
            IProjectsStorage projects)
		{
			this.updateSource = updateSource;
            this.versionProvider = versionProvider;
            this.runner = runner;
            this.requests = requests;
            this.resultsStorage = resultsStorage;
            this.log = log;
            this.projects = projects;
		}
        
        /// <summary>
        /// Runs tests from client
        /// </summary>
        /// <param name="run"></param>
        public TestResult RunTests(TestRun run)
    	{
            if (run == null)
                throw new ArgumentNullException("run");

            TestRunRequest request;
            if ((request = requests.GetBy(run)) == null)
            {
                var result = resultsStorage.GetCompletedResult(run);
                if (result != null)
                    return result.SetFinal();
            }

            request = request ?? requests.AddOrUpdate(run);

            IList<TestResult> availableResults = request.PipeToClient.GetAvailableResults();

            return availableResults == null ? null : availableResults[0];
    	}

        /// <summary>
        /// Gets the update if available.
        /// </summary>
        /// <param name="request"> </param>
        /// <returns></returns>
        public UpdatePackage GetUpdatePackage(UpdateRequest request)
    	{
    		var currentVersion = versionProvider.GetVersion();
    		return new UpdatePackage
			       	{
			       		IsAvailable = request.Version < currentVersion,
						UpdateZipStream = request.Version < currentVersion ? updateSource.GetZippedVersionFolder() : new MemoryStream(),
						Version = currentVersion
			       	};
    	}

        /// <summary>
        /// Receives the project.
        /// </summary>
        /// <param name="project">The project.</param>
        public void ReceiveProject(ProjectMessage project)
        {
            log.BeginActivity(string.Format("Receiving project for test: {0} ({1})", project.TestRun, project.TestRun.NUnitParameters.AssembliesToTest[0]));
    	    
            projects.Store(project.TestRun, project.Project);

            log.EndActivity(string.Format("Received project for test: {0} ({1})", project.TestRun, project.TestRun.NUnitParameters.AssembliesToTest[0]));
        }

        /// <summary>
        /// Determines, whether the specified instance has a project for the given run.
        /// </summary>
        /// <param name="run">The run.</param>
        /// <returns>
        ///   <c>true</c> if the specified instance has a project for the given run; otherwise, <c>false</c>.
        /// </returns>
        public bool HasProject(TestRun run)
        {
            log.BeginActivity(string.Format("Checking for project '{0}'...", run));
            
            bool result = projects.HasProject(run);
            
            log.EndActivity(result ? "Project is available" : "Project was not found on server");

            return result;
        }
    }
}
