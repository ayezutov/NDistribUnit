using System.Collections.Generic;

namespace NDistribUnit.Common.Client
{
    /// <summary>
    /// Parameters, which are same as standard NUnit parameters
    /// </summary>
    public class NUnitParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NUnitParameters"/> class.
        /// </summary>
        public NUnitParameters()
        {
            AssembliesToTest = new List<string>();
            NoShadow = false;
            IncludeCategories = null;
            ExcludeCategories = null;
            TestToRun = null;
        }

        /// <summary>
        /// Gets or sets the test to run.
        /// </summary>
        /// <value>
        /// The test to run.
        /// </value>
        public string TestToRun { get; set; }

        /// <summary>
        /// A collection of all files, which should be tested.
        /// This could be not only assemblies, but a set of NUnit project files (*.nunit)
        /// or Visual Studio project files (*.csproj)
        /// </summary>
        public List<string> AssembliesToTest { get; set; }

        /// <summary>
        /// The name of file for xml output
        /// </summary>
        public string XmlFileName { get; set; }

        /// <summary>
        /// Specifies, that assemblies should not be shadow copied, if set to true.
        /// </summary>
        public bool NoShadow { get; set; }


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
        /// Gets or sets the tests configuration.
        /// </summary>
        /// <value>
        /// The tests configuration.
        /// </value>
        public string Configuration { get; set; }
    }
}