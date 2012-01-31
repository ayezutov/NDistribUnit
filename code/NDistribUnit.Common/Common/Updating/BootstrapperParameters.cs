using System;
using System.IO;

namespace NDistribUnit.Common.Updating
{
	/// <summary>
	/// 
	/// </summary>
	public class BootstrapperParameters
	{
	    private const string bootstrapperDataKey = "ndistribunit.bootstrapped.bootstrapper";
	    private const string bootstrapperConfigurationDataKey = "ndistribunit.bootstrapped.configuration";

	    /// <summary>
		/// Gets or sets the boot strapper path.
		/// </summary>
		/// <value>
		/// The boot strapper path.
		/// </value>
		public string BootstrapperFile { get; set; }

		/// <summary>
		/// Gets or sets the coniguration file.
		/// </summary>
		/// <value>
		/// The coniguration file.
		/// </value>
		public string ConfigurationFile { get; set; }

		/// <summary>
		/// Gets a value indicating whether [all parameters are filled].
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [all parameters are filled]; otherwise, <c>false</c>.
		/// </value>
		public bool AllParametersAreFilled
		{
			get { return !string.IsNullOrEmpty(BootstrapperFile); }
		}

		/// <summary>
		/// Gets the root folder.
		/// </summary>
		public string RootFolder
		{
			get
			{
				return !string.IsNullOrEmpty(BootstrapperFile) 
					? Path.GetDirectoryName(BootstrapperFile) 
					: null;
			}
		}

        /// <summary>
        /// Writes to domain.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="domain">The domain.</param>
	    public static void WriteToDomain(BootstrapperParameters parameters, AppDomain domain)
	    {
            domain.SetData(bootstrapperDataKey, parameters.BootstrapperFile);
            domain.SetData(bootstrapperConfigurationDataKey, parameters.ConfigurationFile);
	    }

        /// <summary>
        /// Inits from domain.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <returns></returns>
	    public static BootstrapperParameters InitFromDomain(AppDomain domain)
	    {
            return new BootstrapperParameters()
                       {
                           BootstrapperFile = (string)domain.GetData(bootstrapperDataKey),
                           ConfigurationFile = (string)domain.GetData(bootstrapperConfigurationDataKey)
                       };
	        
	    }
	}
}