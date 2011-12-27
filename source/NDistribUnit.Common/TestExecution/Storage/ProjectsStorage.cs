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
        /// <param name="loadPackedProject"></param>
        /// <returns></returns>
        public TestProject GetOrLoad(TestRun testRun, Func<PackedProject> loadPackedProject = null)
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
                                               zip.UnpackFolder(ReadBytes(packedFile), unpackedDirectory);
                                               return new TestProject(unpackedDirectory);
                                           }

                                           if (loadPackedProject != null)
                                               return Store(testRun, loadPackedProject());

                                           return null;
                                       });
        }

        /// <summary>
        /// Gets the packed project.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <returns></returns>
        public PackedProject GetPackedProject(TestRun testRun)
        {
            return RunSynchronized(testRun,
                                   () =>
                                       {
                                           var projectPath = GetPathToProject(testRun);

                                           string packedFileName = Path.Combine(projectPath, PackedFileName);
                                           if (File.Exists(packedFileName))
                                               return new PackedProject(packedFileName, ReadBytes(packedFileName));

                                           string unpackedDirectory = Path.Combine(projectPath, UnpackedFolder);
                                           if (Directory.Exists(unpackedDirectory))
                                           {
                                               var packedProject =
                                                   zip.GetPackedFolder(new DirectoryInfo(unpackedDirectory));
                                               var file = File.Create(packedFileName);
                                               file.Write(packedProject, 0, packedProject.Length);
                                               file.Close();
                                               return new PackedProject(packedFileName, packedProject);
                                           }

                                           return null;
                                       });
        }

        /// <summary>
        /// Stores the specified project test run.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        public TestProject Store(TestRun testRun, PackedProject project)
        {
            return RunSynchronized(testRun,
                                   () =>
                                       {
                                           var projectPath = GetPathToProject(testRun);
                                           string packedFileName = Path.Combine(projectPath, PackedFileName);

                                           if (!Directory.Exists(projectPath))
                                               Directory.CreateDirectory(projectPath);

                                           var file = File.Create(packedFileName);
                                           file.Write(project.Data, 0, project.Data.Length);
                                           file.Close();
                                           return GetOrLoad(testRun);
                                       });
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
    }
}