using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using NDistribUnit.Common.Common;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Updating;
using NUnit.Core;
using System.Linq;

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
        private readonly ConcurrentDictionary<Guid, ReaderWriterLockSlim> lockers = new ConcurrentDictionary<Guid, ReaderWriterLockSlim>();

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
        /// <param name="result">The result.</param>
        /// <param name="testRun"> </param>
        public void Add(TestResult result, TestRun testRun)
        {
            var resultsForTestRun = GetCollection(testRun);
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

            RunResultsCollection temp;
            results.TryRemove(testRun.Id, out temp);

            var result = resultsForTestRun.Close();
            try
            {
                Store(testRun, resultsForTestRun.MergedResult);
            }
            catch (Exception ex)
            {
                log.Error("Exception while saving tests:", ex);
            }
            
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

            var locker = GetLocker(testRun);

            try
            {
                locker.EnterReadLock();

                var binaryFile = GetBinaryFileName(testRun);

                if (!File.Exists(binaryFile))
                    return null;

                return serializer.ReadBinary(new FileStream(binaryFile, FileMode.Open, FileAccess.Read));
            }
            finally
            {
                locker.ExitReadLock();
            }
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

            var locker = GetLocker(testRun);
            try
            {
                locker.EnterWriteLock();

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
            finally
            {
                locker.ExitWriteLock();
            }
        }

        private ReaderWriterLockSlim GetLocker(TestRun testRun)
        {
            return lockers.GetOrAdd(testRun.Id, guid => new ReaderWriterLockSlim());
        }

        private string GetResultsStorageFolderName(TestRun testRun)
        {
            //var now = DateTime.Now;
            //var dateFolderName = now.ToString("yyyy-MM-dd");
            //var leafFolderName = PathUtilities.EscapeFileName(string.Format("{0}-{1}{2}", now.ToString("HH-mm-ss"), testRun.Id, !string.IsNullOrEmpty(testRun.Alias) ? "-"+testRun.Alias : string.Empty));
            return Path.Combine(parameters.RootFolder, folderName, testRun.Id.ToString()/*dateFolderName, leafFolderName*/);
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