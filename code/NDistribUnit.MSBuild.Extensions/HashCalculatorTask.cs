using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NDistribUnit.Common.HashChecks;
using System.Linq;

namespace NDistribUnit.MSBuild.Extensions
{
	[LoadInSeparateAppDomainAttribute]
	public class HashCalculatorTask: AppDomainIsolatedTask
	{
		/// <summary>
		/// Gets or sets the path.
		/// </summary>
		/// <value>
		/// The path.
		/// </value>
		public string Path { get; set; }

		/// <summary>
		/// Gets or sets the files to ignore.
		/// </summary>
		/// <value>
		/// The files to ignore.
		/// </value>
		public ITaskItem[] FilesToIgnore { get; set; }

		/// <summary>
		/// Gets or sets the files with any contents.
		/// </summary>
		/// <value>
		/// The files with any contents.
		/// </value>
		public ITaskItem[] FilesWithAnyContents { get; set; }

		/// <summary>
		/// Runs the task.
		/// </summary>
		/// <returns>
		/// true if successful; otherwise, false.
		/// </returns>
		public override bool Execute()
		{
			IList<string> filesToIgnore = null;
			IEnumerable<string> mandatoryFilesWithAnyContent = null;

			if (FilesToIgnore != null)
				filesToIgnore = FilesToIgnore.Select(item => item.ItemSpec).ToList();

			if (FilesWithAnyContents != null)
				mandatoryFilesWithAnyContent = FilesWithAnyContents.Select(item => item.ItemSpec);

			var directoryHash = new DirectoryHash(Path, filesToIgnore, mandatoryFilesWithAnyContent);
			
			directoryHash.Save();
			return true;
		}
	}
}