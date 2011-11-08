using System;
using System.Collections.Generic;
using NDistribUnit.Common.ConsoleProcessing.Options;

namespace NDistribUnit.Common.Client
{
    /// <summary>
    /// Holds all the parsed command line parameters for NDistribUnit client
    /// </summary>
    [Serializable]
    public class ClientParameters
    {
        /// <summary>
        /// Initializes a new instance
        /// </summary>
        public ClientParameters()
        {
            AssembliesToTest = new List<string>();
            UnknownOptions = new List<ConsoleOption>();
            NoShadow = false;
            IncludeCategories = null;
            ExcludeCategories = null;
            TimeoutPeriod = TimeSpan.FromHours(5);
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
        private List<ConsoleOption> UnknownOptions { get; set; }

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
					{"include", (string includes) => result.IncludeCategories = includes },
					{"exclude", (string excludes) => result.ExcludeCategories = excludes },
					{"alias", (string alias) => result.Alias = alias },
						
                    {ConsoleOption.UnnamedOptionName, (string assembly) => result.AssembliesToTest.Add(assembly), false},
                };

            result.UnknownOptions.AddRange(set.Parse(arguments));
            return result;
        }

        /// <summary>
        /// Gets or sets the alias.
        /// </summary>
        /// <value>
        /// The alias.
        /// </value>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the categories to exclude.
        /// </summary>
        /// <value>
        /// The exclude categories.
        /// </value>
        public string ExcludeCategories { get; set; }

        /// <summary>
        /// Gets or sets the categories to include.
        /// </summary>
        /// <value>
        /// The include categories.
        /// </value>
        public string IncludeCategories { get; set; }

        /// <summary>
        /// Gets or sets the timeout period.
        /// </summary>
        /// <value>
        /// The timeout period.
        /// </value>
        public TimeSpan TimeoutPeriod { get; set; }
    }
}