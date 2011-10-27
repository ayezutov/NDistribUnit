using System;
using System.Collections.Generic;
using NDistribUnit.Common.ConsoleProcessing.Options;
using System.Linq;

namespace NDistribUnit.Common.Client
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
            IncludeCategories = new string[0];
            ExcludeCategories = new string[0];
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
    	/// Gets or sets the server URI.
    	/// </summary>
    	/// <value>
    	/// The server URI.
    	/// </value>
    	public Uri ServerUri { get; set; }

		/// <summary>
		/// Gets or sets the tests configuration.
		/// </summary>
		/// <value>
		/// The tests configuration.
		/// </value>
    	public string Configuration { get; set; }

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
                    {"configuration", (string configuration) => result.Configuration = configuration, false },
					{"server", (string testServerUri) => result.ServerUri = Uri.IsWellFormedUriString(testServerUri, UriKind.Absolute) 
                        ? new Uri(testServerUri)
						: null, false},
					{"include", (string includes) => result.IncludeCategories = ParseCategories(includes) },
					{"exclude", (string excludes) => result.ExcludeCategories = ParseCategories(excludes) },
					{"alias", (string alias) => result.Alias = alias },
						
                    {ConsoleOption.UnnamedOptionName, (string assembly) => result.AssembliesToTest.Add(assembly), false},
                };

            result.UnknownOption.AddRange(set.Parse(arguments));
            return result;
        }

        /// <summary>
        /// Gets or sets the alias.
        /// </summary>
        /// <value>
        /// The alias.
        /// </value>
        public string Alias { get; set; }

        private static string[] ParseCategories(string categories)
        {
            return categories.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries).Select(category => category.Trim()).ToArray();
        }

        /// <summary>
        /// Gets or sets the categories to exclude.
        /// </summary>
        /// <value>
        /// The exclude categories.
        /// </value>
        public string[] ExcludeCategories { get; set; }

        /// <summary>
        /// Gets or sets the categories to include.
        /// </summary>
        /// <value>
        /// The include categories.
        /// </value>
        public string[] IncludeCategories { get; set; }
    }
}