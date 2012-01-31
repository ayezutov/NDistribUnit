using System;
using System.IO;
using System.Threading;
using NDistribUnit.Common.Common;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Contracts.DataContracts;
using NDistribUnit.Common.Logging;
using NDistribUnit.Common.Updating;

namespace NDistribUnit.Common.TestExecution.Storage
{
    /// <summary>
    /// The storage on server or agent
    /// </summary>
    public class ProjectsStorage : IProjectsStorage
    {
        private const string PackedFileName = "packed.zip";
        private const string UnpackedFolder = "unpacked";
        private const string StorageFolderName = "_Storage";
        private const string TemporaryStorageFolderName = "Temporary";
        private const string PermanentStorageFolderName = "Permanent";

        private readonly string storageName;
        private readonly BootstrapperParameters parameters;
        private readonly ZipSource zip;
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectsStorage"/> class.
        /// </summary>
        /// <param name="storageName">Name of the storage.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="zip">The zip.</param>
        /// <param name="log">The log.</param>
        public ProjectsStorage(string storageName, BootstrapperParameters parameters, ZipSource zip, ILog log)
        {
            this.storageName = storageName;
            this.parameters = parameters;
            this.zip = zip;
            this.log = log;
        }

        /// <summary>
        /// Gets the root path.
        /// </summary>
        private string RootPath
        {
            get { return parameters.RootFolder; }
        }


        private string GetPathToProject(TestRun testRun)
        {
            if (!testRun.IsAliasedTest)
                return Path.Combine(RootPath, GetStorageFolderName(), TemporaryStorageFolderName, testRun.Id.ToString());
            return Path.Combine(RootPath, GetStorageFolderName(), PermanentStorageFolderName,
                                PathUtilities.EscapeFileName(testRun.Alias));
        }

        private string GetStorageFolderName()
        {
            return string.IsNullOrEmpty(storageName)
                       ? StorageFolderName
                       : string.Format("{0}.{1}", StorageFolderName, storageName);
        }

        /// <summary>
        /// Runs the synchronized.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <param name="func">The action.</param>
        private T RunSynchronized<T>(TestRun testRun, Func<T> func)
        {
            var mutex = new Mutex(false, testRun.Id.ToString());
            mutex.WaitOne();
            try
            {
                return func();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Runs the synchronized.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <param name="action">The action.</param>
        private void RunSynchronized(TestRun testRun, Action action)
        {
            RunSynchronized(testRun, () =>
                                         {
                                             action();
                                             return true;
                                         });
        }

        /// <summary>
        /// Determines whether the specified test run has project.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <returns>
        ///   <c>true</c> if the specified test run has project; otherwise, <c>false</c>.
        /// </returns>
        public bool HasProject(TestRun testRun)
        {
            return Get(testRun) != null;
        }

        /// <summary>
        /// Gets the project.
        /// </summary>
        /// <param name="testRun"></param>
        /// <returns></returns>
        public TestProject Get(TestRun testRun)
        {
            return RunSynchronized(testRun,
                                   () =>
                                       {
                                           string path = GetPathToProject(testRun);

                                           string unpackedDirectory = Path.Combine(path, UnpackedFolder);

                                           if (Directory.Exists(unpackedDirectory))
                                               return new TestProject(unpackedDirectory);

                                           string packedFile = Path.Combine(path, PackedFileName);
                                           return Unpack(packedFile, unpackedDirectory);
                                       });
        }

        private TestProject Unpack(string packedFile, string unpackedDirectory)
        {
            if (File.Exists(packedFile))
            {
                using (FileStream fileStream = File.OpenRead(packedFile))
                {
                    zip.UnpackFolder(fileStream, unpackedDirectory);
                }
                return new TestProject(unpackedDirectory);
            }
            return null;
        }

        /// <summary>
        /// Stores the specified test run.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <param name="projectStream">The project stream.</param>
        public void Store(TestRun testRun, Stream projectStream)
        {
            RunSynchronized(testRun, () =>
                                         {
                                             var path = GetPathToProject(testRun);
                                             var packedFileName = Path.Combine(path, PackedFileName);
                                             var fileName = packedFileName;
                                             if (!Directory.Exists(path))
                                                 Directory.CreateDirectory(path);

                                             if (File.Exists(fileName))
                                                 fileName = Path.GetTempFileName();

                                             var file = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                                             var buffer = new byte[1024*1024];
                                             int readBytes;
                                             while ((readBytes = projectStream.Read(buffer, 0, buffer.Length)) > 0)
                                             {
                                                 file.Write(buffer, 0, readBytes);
                                             }
                                             file.Close();

                                             if (!fileName.Equals(packedFileName))
                                             {
                                                 if (new FileInfo(fileName).Length != new FileInfo(packedFileName).Length)
                                                 {
                                                     try
                                                     {
                                                         File.Delete(packedFileName);
                                                     }
                                                     catch(IOException ex)
                                                     {
                                                         log.Warning("Error while trying to replace the file", ex);
                                                         return;
                                                     }
                                                     try
                                                     {
                                                         File.Move(fileName, packedFileName);
                                                     }
                                                     catch(IOException ex)
                                                     {
                                                         log.Warning("Error while trying to move a file", ex);
                                                     }
                                                 }
                                             }
                                         });
        }

        /// <summary>
        /// Gets the stream to packed.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <returns></returns>
        public Stream GetStreamToPacked(TestRun testRun)
        {
            return RunSynchronized(testRun,
                                   () =>
                                       {
                                           string path = GetPathToProject(testRun);

                                           string packedFile = Path.Combine(path, PackedFileName);

                                           if (File.Exists(packedFile))
                                               return File.OpenRead(packedFile);

                                           string unpackedDirectory = Path.Combine(path, UnpackedFolder);

                                           if (Directory.Exists(unpackedDirectory))
                                           {
                                               using (FileStream fileStream = File.Create(packedFile))
                                               {
                                                   return zip.GetPackedFolder(new DirectoryInfo(unpackedDirectory), true,
                                                                       fileStream);
                                               }
                                           }

                                           return null;
                                       });
        }

        /// <summary>
        /// Removes the project.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        public void RemoveProject(TestRun testRun)
        {
            Action<Action> withLog = a =>
                                         {
                                             try
                                             {
                                                 a();
                                             }
                                             catch (IOException ex)
                                             {
                                                 log.Warning(string.Format("Exception while cleaning project {0}", testRun), ex);
                                             }
                                         };
            RunSynchronized(testRun, () =>
                                         {
                                             string path = GetPathToProject(testRun);

                                             string packedFile = Path.Combine(path, PackedFileName);

                                             if (File.Exists(packedFile))
                                             {
                                                 withLog(()=>File.Delete(packedFile));
                                             }

                                             string unpackedDirectory = Path.Combine(path, UnpackedFolder);

                                             if (Directory.Exists(unpackedDirectory))
                                             {
                                                 withLog(()=>Directory.Delete(unpackedDirectory, true));
                                             }

                                             if (Directory.Exists(path))
                                                withLog(()=>Directory.Delete(path));
                                         });
        }


    }
}