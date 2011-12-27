using System;
using System.Collections.Concurrent;
using System.IO;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Updating;
using NUnit.Core;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// A default implementation of results storage
    /// </summary>
    public class ResultsStorage : IResultsStorage
    {
        private readonly TestResultsProcessor processor;
        private readonly ITestResultsSerializer serializer;
        private readonly ILog log;
        private readonly BootstrapperParameters parameters;
        private readonly string folderName;
        private readonly ConcurrentDictionary<Guid, RunResultsCollection> results = new ConcurrentDictionary<Guid, RunResultsCollection>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultsStorage"/> class.
        /// </summary>
        /// <param name="processor">The processor.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="log">The log.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="folderName">Name of the folder.</param>
        public ResultsStorage(TestResultsProcessor processor, ITestResultsSerializer serializer, ILog log, BootstrapperParameters parameters, string folderName)
        {
            this.processor = processor;
            this.serializer = serializer;
            this.log = log;
            this.parameters = parameters;
            this.folderName = folderName;
        }

        /// <summary>
        /// Adds the specified result.
        /// </summary>
        /// <param name="test"></param>
        /// <param name="result">The result.</param>
        public void Add(TestUnitWithMetadata test, TestUnitResult result)
        {
            var resultsForTestRun = GetCollection(test.Test.Run);
            resultsForTestRun.AddUnmerged(result);
        }

        private RunResultsCollection GetCollection(TestRun testRun)
        {
            return results.GetOrAdd(testRun.Id, guid => new RunResultsCollection(processor, log));
        }

        /// <summary>
        /// Stores as completed.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <returns></returns>
        public TestResult StoreAsCompleted(TestRun testRun)
        {
            var resultsForTestRun = GetCollection(testRun);
            var result = resultsForTestRun.Close();
            Store(testRun, resultsForTestRun.MergedResult);
            return result;
        }

        /// <summary>
        /// Gets the completed result.
        /// </summary>
        /// <param name="testRun">The run.</param>
        /// <returns></returns>
        public TestResult GetCompletedResult(TestRun testRun)
        {
            RunResultsCollection result;
            results.TryGetValue(testRun.Id, out result);

            if (result != null)
                return null; // the run has not completed yet

            var binaryFile = GetBinaryFileName(testRun);

            if (!File.Exists(binaryFile))
                return null;

            return serializer.ReadBinary(new FileStream(binaryFile, FileMode.Open));
        }

        /// <summary>
        /// Stores the specified result.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <param name="mergedResult"></param>
        protected virtual void Store(TestRun testRun, TestResult mergedResult)
        {
            var folder = GetResultsStorageFolderName(testRun);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var xmlFile = GetXmlFileName(testRun);
            var xml = serializer.GetXml(mergedResult);
            var xmlStream = new StreamWriter(xmlFile, false);
            xmlStream.Write(xml);
            xmlStream.Close();

            var binaryFile = GetBinaryFileName(testRun);
            var dataStream = new FileStream(binaryFile, FileMode.Create);
            serializer.WriteBinary(mergedResult, dataStream);
            dataStream.Close();
        }

        private string GetResultsStorageFolderName(TestRun testRun)
        {
            return Path.Combine(parameters.RootFolder, folderName, testRun.Id.ToString());
        }

        private string GetXmlFileName(TestRun testRun)
        {
            return Path.Combine(GetResultsStorageFolderName(testRun), string.Format("results.xml"));
        }

        private string GetBinaryFileName(TestRun testRun)
        {
            return Path.Combine(GetResultsStorageFolderName(testRun), string.Format("results.bin"));
        }
    }
}