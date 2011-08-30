using System;
using System.Collections.Generic;
using System.IO;
using NDistribUnit.Common.ConsoleProcessing.Options;

namespace NDistribUnit.Common.Updating
{
	/// <summary>
	/// 
	/// </summary>
	public class BootstrapperParameters
	{
		private const string bootstrapperFileKeyName = "bootstrapperFile";
		private const string configurationFileKeyName = "configurationFile";
		private const string isDebugKeyName = "isDebug";

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
			get { return !string.IsNullOrEmpty(BootstrapperFile) && !string.IsNullOrEmpty(ConfigurationFile); }
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
		/// Parses the specified arguments.
		/// </summary>
		/// <param name="arguments">The arguments.</param>
		/// <returns></returns>
		public static BootstrapperParameters Parse(IEnumerable<string> arguments)
		{
			var result = new BootstrapperParameters();
			new ConsoleParametersParser
				{
					{bootstrapperFileKeyName, (string bootstrapperFile) => result.BootstrapperFile = bootstrapperFile},
					{configurationFileKeyName, (string configurationFile) => result.ConfigurationFile = configurationFile}
				}.Parse(arguments);

			return result;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format("/{0}:{1} /{2}:{3}", bootstrapperFileKeyName, BootstrapperFile, 
				configurationFileKeyName, ConfigurationFile);
		}

		/// <summary>
		/// Toes the array.
		/// </summary>
		/// <returns></returns>
		public string[] ToArray()
		{
			
			return new string[]
			       	{
			       		string.Format("/{0}:{1}", bootstrapperFileKeyName, BootstrapperFile),
						string.Format("/{0}:{1}", configurationFileKeyName, ConfigurationFile)
			       	};

		}
	}
}