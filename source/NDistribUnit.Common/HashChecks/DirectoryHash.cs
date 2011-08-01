using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using System.Text;

namespace NDistribUnit.Common.HashChecks
{
	/// <summary>
	/// Represents a hash for a directory
	/// </summary>
	public class DirectoryHash
	{
		private readonly IEnumerable<string> mandatoryFilesWithAnyContent;
		private readonly DirectoryInfo rootDirectory;
		private readonly IList<string> filesToIgnore;
		private IList<Tuple<string, string>> hash;
		private const string wholeHashKey = ":::";
		private const string fileName = "directory.hash";

		/// <summary>
		/// Initializes a new instance of the <see cref="DirectoryHash"/> class.
		/// </summary>
		/// <param name="rootDirectoryName">Name of the root directory.</param>
		/// <param name="filesToIgnore">The files to ignore.</param>
		/// <param name="mandatoryFilesWithAnyContent">List of mandatory files with any content.</param>
		public DirectoryHash(string rootDirectoryName, IList<string> filesToIgnore = null, IEnumerable<string> mandatoryFilesWithAnyContent = null)
			: this(new DirectoryInfo(rootDirectoryName), filesToIgnore)
		{
			this.mandatoryFilesWithAnyContent = mandatoryFilesWithAnyContent;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DirectoryHash"/> class.
		/// </summary>
		/// <param name="rootDirectory">The directory.</param>
		/// <param name="filesToIgnore">The files to ignore.</param>
		public DirectoryHash(DirectoryInfo rootDirectory, IList<string> filesToIgnore = null)
		{
			if (filesToIgnore == null)
				filesToIgnore = new List<string>();
			if (!filesToIgnore.Contains(fileName))
				filesToIgnore.Add(fileName);

			this.rootDirectory = rootDirectory;
			this.filesToIgnore = filesToIgnore;
		}

		/// <summary>
		/// Gets the hash.
		/// </summary>
		public IList<Tuple<string, string>> Hash
		{
			get { return hash ?? (hash = Calculate(rootDirectory)); }
		}

		/// <summary>
		/// Calculates this instance.
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <param name="result">The result.</param>
		/// <param name="sb"></param>
		/// <returns></returns>
		private IList<Tuple<string, string>> Calculate(DirectoryInfo directory, IList<Tuple<string, string>> result = null, StringBuilder sb = null)
		{
			var isRoot = (sb == null && result == null);
			if (sb == null)
				sb = new StringBuilder();

			if (result == null)
				result = new List<Tuple<string, string>>();

			foreach (var file in directory.GetFiles())
			{
				string relativeName = file.FullName.Replace(rootDirectory.FullName, string.Empty).Trim('\\', '/');

				if (filesToIgnore.Any(filename => filename.Equals(relativeName, StringComparison.OrdinalIgnoreCase) ||
					filename.Equals(file.FullName, StringComparison.OrdinalIgnoreCase)))
					continue;

				string fileHash = mandatoryFilesWithAnyContent != null &&
				                  mandatoryFilesWithAnyContent.Any(
				                  	filename => filename.Equals(relativeName, StringComparison.OrdinalIgnoreCase) ||
				                  	            filename.Equals(file.FullName, StringComparison.OrdinalIgnoreCase))
				                  	? string.Empty
				                  	: GetFileHash(file);

				result.Add(new Tuple<string, string>(relativeName, fileHash));
			}

			foreach (var subDirectory in directory.GetDirectories())
			{
				Calculate(subDirectory, result, sb);
			}
			if (isRoot)
			{
				var wholeHash = RecalculateWholeHash(result);
				
					
				result.Add(new Tuple<string, string>(wholeHashKey, wholeHash));
			}
			return result;
		}

		private string GetFileHash(FileInfo file)
		{
			using (var fileStream = new FileStream(file.FullName, FileMode.Open))
			{
				var provider = new SHA1CryptoServiceProvider();
				byte[] hashArray = provider.ComputeHash(fileStream);
				return BitConverter.ToString(hashArray);
			}
		}

		/// <summary>
		/// Saves to.
		/// </summary>
		public void Save()
		{
			var result = Hash;

			using (var file = new StreamWriter(new FileStream(Path.Combine(rootDirectory.FullName, fileName), FileMode.Create)))
			{
				foreach (var tuple in result)
				{
					file.WriteLine("{0}\t{1}", tuple.Item1, tuple.Item2);
				}
			}
		}

		/// <summary>
		/// Validates this instance.
		/// </summary>
		/// <returns></returns>
		public void Validate()
		{
			var original = LoadFromFile(fileName);
			if (original == null)
				throw new HashValidationException(new []{string.Format("File '{0}' was not found in '{1}'", fileName, rootDirectory.FullName)});

			ValidateHashFileIntegrity(original);

			var current = Calculate(rootDirectory);

			var result = new List<string>();
			foreach (var orig in original)
			{
				var recalculated = current.FirstOrDefault(cur => cur.Item1.Equals(orig.Item1, StringComparison.OrdinalIgnoreCase));
				if (recalculated == null)
					result.Add(string.Format("File {0} is absent", orig.Item1));
				else if (!string.IsNullOrEmpty(orig.Item2) && !recalculated.Item2.Equals(orig.Item2))
					result.Add(string.Format("The hash '{1}' of file '{0}' doesn't match the saved '{2}'",
																	  recalculated.Item1, recalculated.Item2, orig.Item2));
			}

			if (result.Count == 0)
				return;

			throw new HashValidationException(result);
		}

		private void ValidateHashFileIntegrity(IList<Tuple<string, string>> loadedHash)
		{
			Tuple<string, string> wholeHashTuple = loadedHash.FirstOrDefault(t => t.Item1.Equals(wholeHashKey));
			if (wholeHashTuple == null)
				throw new HashValidationException("The hash for the hash file is absent. Hash file is invalid");

			loadedHash.Remove(wholeHashTuple);

			var recalculatedHash = RecalculateWholeHash(loadedHash);

			if (!wholeHashTuple.Item2.Equals(recalculatedHash))
				throw new HashValidationException(string.Format("The hash of 'directory.hash' file ('{0}') doesn't match the expected '{1}'", recalculatedHash, wholeHashTuple.Item2));
		}

		private string RecalculateWholeHash(IEnumerable<Tuple<string, string>> hashToCalculate)
		{
			var sb = new StringBuilder();

			foreach (var hashItems in hashToCalculate)
			{
				if (hashItems.Item1.Equals(wholeHashKey))
					continue;

				sb.Append(hashItems.Item1);
				sb.AppendLine(hashItems.Item2);
			}

			return BitConverter.ToString(new SHA1CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(sb.ToString())));
				
		}

		private IList<Tuple<string, string>> LoadFromFile(string fileName)
		{
			string fullFileName = Path.Combine(rootDirectory.FullName, fileName);
			if (!File.Exists(fullFileName))
				return null;

			var result = new List<Tuple<string, string>>();
			using (var reader = new StreamReader(new FileStream(fullFileName, FileMode.Open)))
			{
				while (!reader.EndOfStream)
				{
					var lineSplit = reader.ReadLine().Split('\t');
					result.Add(new Tuple<string, string>(lineSplit[0], lineSplit.Length > 1 ? lineSplit[1] : string.Empty));
				}
			}
			if (result.Count == 0)
				return null;
			return result;
		}
	}
}