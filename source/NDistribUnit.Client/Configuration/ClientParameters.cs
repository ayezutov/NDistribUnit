using System.Collections.Generic;
using NDistribUnit.Common.ConsoleProcessing.Options;

namespace NDistribUnit.Client.Configuration
{
    /// <summary>
    /// Holds all the parsed command line parameters for NDistribUnit client
    /// </summary>
    public class ClientParameters
    {
        /// <summary>
        /// Initializes a new instance
        /// </summary>
        public ClientParameters()
        {
            AssembliesToTest = new List<string>();
            UnknownOption = new List<ConsoleOption>();
            NoShadow = false;
        }

        /// <summary>
        /// A collection of all files, which should be tested.
        /// This could be not only assemblies, but a set of NUnit project files (*.nunit)
        /// or Visual Studio project files (*.csproj)
        /// </summary>
        public List<string> AssembliesToTest { get; private set; }

        /// <summary>
        /// The name of file for xml output
        /// </summary>
        public string XmlFileName { get; set; }

        /// <summary>
        /// Specifies, that assemblies should not be shadow copied, if set to true.
        /// </summary>
        public bool NoShadow { get; set; }

        /// <summary>
        /// A collection of all unknown options, which were detected during parsing
        /// </summary>
        private List<ConsoleOption> UnknownOption { get; set; }

        /// <summary>
        /// Parses the command line returning a typed 
        /// options object
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static ClientParameters Parse(IEnumerable<string> arguments)
        {
            var result = new ClientParameters();
            var set = new ConsoleParametersParser
                {
                    {"xml", (string xmlFileName) => result.XmlFileName = xmlFileName, false },
                    {"noshadow", (bool noShadow) => result.NoShadow = noShadow, true },
                    {ConsoleOption.UnnamedOptionName, (string assembly) => result.AssembliesToTest.Add(assembly), false}
                };

            result.UnknownOption.AddRange(set.Parse(arguments));
            return result;
        }
    }
}