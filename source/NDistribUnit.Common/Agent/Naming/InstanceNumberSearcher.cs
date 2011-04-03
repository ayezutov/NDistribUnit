using System.IO;
using System.Reflection;
using System.Threading;

namespace NDistribUnit.Common.Agent.Naming
{
    /// <summary>
    /// A name provider, which provides a prefix with the first free number for the current asssembly on this machine.
    /// </summary>
    public static class InstanceNumberSearcher
    {
        private static int number;
// ReSharper disable NotAccessedField.Local
        private static Mutex mutex; // this reference should live till the end of the program. Mutex will be released on process termination.
// ReSharper restore NotAccessedField.Local

        /// <summary>
        /// Gets the name.
        /// </summary>
        public static int Number
        {
            get 
            { 
                if (number == 0)
                {
                    number = GetNumber();
                }

                return number;
            }
        }
        
        private static int GetNumber()
        {
            var result = 0;

            string mutexPrefix = "NDistribUnit.Agent";

            Assembly assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                // in NUnit tests
                string directoryName = Path.GetDirectoryName(assembly.CodeBase);

                if (directoryName != null)
                    mutexPrefix = directoryName.Replace('\\', '_');
            }

            while (true)
            {
                result++;
                
                string name = string.Format("{0}::{1}", mutexPrefix, result);
                
                bool wasCreated;
                mutex = new Mutex(true, name, out wasCreated);

                if (wasCreated)
                    break;
            }

            return result;
        }
    }
}