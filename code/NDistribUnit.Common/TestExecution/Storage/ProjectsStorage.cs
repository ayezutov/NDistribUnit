using System;
using System.IO;
using System.Threading;
using NDistribUnit.Common.Common.Communication;
using NDistribUnit.Common.Contracts.DataContracts;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectsStorage"/> class.
        /// </summary>
        /// <param name="storageName">Name of the storage.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="zip">The zip.</param>
        public ProjectsStorage(string storageName, BootstrapperParameters parameters, ZipSource zip)
        {
            this.storageName = storageName;
            this.parameters = parameters;
            this.zip = zip;
        }

        /// <summary>
        /// Gets the root path.
        /// </summary>
        private string RootPath
        {
            get { return parameters.RootFolder; }
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
                                           if (File.Exists(packedFile))
                                           {
                                               using(FileStream fileStream = File.OpenRead(packedFile))
                                               {
                                                   zip.UnpackFolder(fileStream, unpackedDirectory);
                                               }
                                               return new TestProject(unpackedDirectory);
                                           }
                                           
                                           return null;
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

        private string GetPathToProject(TestRun testRun)
        {
            if (string.IsNullOrEmpty(testRun.Alias))
                return Path.Combine(RootPath, GetStorageFolderName(), TemporaryStorageFolderName, testRun.Id.ToString());
            return Path.Combine(RootPath, GetStorageFolderName(), PermanentStorageFolderName, testRun.Alias);
        }

        private string GetStorageFolderName()
        {
            return string.IsNullOrEmpty(storageName)
                       ? StorageFolderName
                       : string.Format("{0}.{1}", StorageFolderName, storageName);
        }

        private static byte[] ReadBytes(string fullFilePath)
        {
            FileStream fs = File.OpenRead(fullFilePath);
            try
            {
                var bytes = new byte[fs.Length];
                fs.Read(bytes, 0, (int) fs.Length);
                fs.Close();
                return bytes;
            }
            finally
            {
                fs.Close();
            }
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
        /// Stores the specified test run.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <param name="projectStream">The project stream.</param>
        public void Store(TestRun testRun, Stream projectStream)
        {
            var path = GetPathToProject(testRun);
            var packedFile = Path.Combine(path, PackedFileName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var file = new FileStream(packedFile, FileMode.CreateNew, FileAccess.Write);
            var buffer = new byte[1024*1024];
            int readBytes;
            while ((readBytes = projectStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                file.Write(buffer, 0, readBytes);
            }
            file.Close();
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
                                               zip.GetPackedFolder(new DirectoryInfo(unpackedDirectory), true, fileStream);
                                           }
                                           return File.OpenRead(packedFile);
                                       }

                                       return null;
                                   });
        }
    }
}