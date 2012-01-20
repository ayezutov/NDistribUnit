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
            NUnitParameters = new NUnitParameters();
            UnknownOptions = new List<ConsoleOption>();
            TimeoutPeriod = TimeSpan.FromHours(5);
        }

        /// <summary>
        /// Gets the NUnit parameters.
        /// </summary>
        /// <value>
        /// The N unit parameters.
        /// </value>
        public NUnitParameters NUnitParameters { get; private set; } 

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
                    {"xml", (string xmlFileName) => result.NUnitParameters.XmlFileName = xmlFileName, false },
                    {"noshadow", (bool noShadow) => result.NUnitParameters.NoShadow = noShadow, true },
                    {"configuration", (string configuration) => result.NUnitParameters.Configuration = configuration, false },
					{"server", (string testServerUri) => result.ServerUri = Uri.IsWellFormedUriString(testServerUri, UriKind.Absolute) 
                        ? new Uri(testServerUri)
						: null, false},
					{"include", (string includes) => result.NUnitParameters.IncludeCategories = includes },
					{"exclude", (string excludes) => result.NUnitParameters.ExcludeCategories = excludes },
					{"alias", (string alias) => result.Alias = alias },
					{"run", (string run) => result.NUnitParameters.TestToRun = run},
                    {ConsoleOption.UnnamedOptionName, (string assembly) => result.NUnitParameters.AssembliesToTest.Add(assembly), false},
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
        /// Gets or sets the timeout period.
        /// </summary>
        /// <value>
        /// The timeout period.
        /// </value>
        public TimeSpan TimeoutPeriod { get; set; }
    }
}