using System.IO;
using System.Reflection;
using System.Threading;

namespace NDistribUnit.Agent.Naming
{
    /// <summary>
    /// A name provider, which provides a prefix with the first free number for the current asssembly on this machine.
    /// </summary>
    public class FreeNumberNameProvider : INameProvider
    {
        private static int number = 0;
        private static Mutex mutex;

        /// <summary>
        /// Initializes a new instance of the <see cref="FreeNumberNameProvider"/> class.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        public FreeNumberNameProvider(string prefix)
        {
            Prefix = prefix;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name
        {
            get 
            { 
                if (number == 0)
                {
                    number = GetNumber();
                }

                return string.Format("{0} #{1:000}", Prefix, number);
            }
        }

        private string Prefix { get; set; }


        private static int GetNumber()
        {
            var result = 0;
            while (true)
            {
                result++;

                string name = string.Format("{0}::{1}", Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase).Replace(Path.DirectorySeparatorChar, '_'), result);


                bool wasCreated;
                mutex = new Mutex(true, name, out wasCreated);

                if (wasCreated)
                    break;

            }

            return result;
        }
    }
}