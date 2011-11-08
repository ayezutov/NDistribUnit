using NUnit.Core;
using NUnit.Util;

namespace NDistribUnit.Common.TestExecution
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITestSystemInitializer
    {
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        void Initialize();
    }

    /// <summary>
    /// A class, which initializes the test system
    /// </summary>
    public class NUnitInitializer : ITestSystemInitializer
    {
        private static bool IsInitialized { get; set; }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
            public void Initialize()
            {
                if (IsInitialized)
                    return;
                lock (GetType())
                {
                    if (IsInitialized)
                        return;
                    CoreExtensions.Host.InstallBuiltins();
                    ServiceManager.Services.AddService(new SettingsService());
                    ServiceManager.Services.AddService(new DomainManager());
                    ServiceManager.Services.AddService(new ProjectService());
                    ServiceManager.Services.AddService(new TestLoader());
                    ServiceManager.Services.AddService(new AddinRegistry());
                    ServiceManager.Services.AddService(new AddinManager());
                    ServiceManager.Services.AddService(new TestAgency());
                    ServiceManager.Services.InitializeServices();
                    IsInitialized = true;
                }

            }
    }
}