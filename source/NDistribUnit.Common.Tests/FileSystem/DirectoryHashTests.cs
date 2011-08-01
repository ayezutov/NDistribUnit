using System;
using System.IO;
using System.Reflection;
using System.Text;
using NDistribUnit.Common.HashChecks;
using NUnit.Framework;

namespace NDistribUnit.Common.Tests.FileSystem
{
    [TestFixture]
    public class DirectoryHashTests
    {
    	private string location;
    	private readonly Random random = new Random();

    	[TestFixtureSetUp]
		public void PrepareDirectory()
		{
			location = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "temporaryDirectory");
    		var directoryInfo = new DirectoryInfo(location);
			if (!directoryInfo.Exists)
				directoryInfo.Create();
    		CreateFile("someRootFile.exe");
    		CreateFile("folder1\\someFile.txt");
    		CreateFile("folder1\\folder2\\someFile2.asd");
    		CreateFile("folder1\\folder2\\someFile2.fdd");
		}

		[SetUp]
		public void ClearDirectory()
		{
			string hashFile = Path.Combine(location, "directory.hash");
			if (File.Exists(hashFile))
				File.Delete(hashFile);
		}

    	private void CreateFile(string fileName)
    	{
    		var file = new FileInfo(Path.Combine(location, fileName));
    		var fileDir = file.Directory;
			if (!fileDir.Exists)
				fileDir.Create();

    		var stream = file.Create();
    		var count = random.Next(100, 20000);
    		var buffer = new byte[count];
    		random.NextBytes(buffer);
			stream.Write(buffer, 0, count);
			stream.Close();
    	}

    	[Test]
		public void HashIsCalculated()
    	{
    		var directoryHash = new DirectoryHash(location);
			Assert.That(directoryHash.Hash.Count, Is.Not.EqualTo(0));
    	}

    	[Test]
		public void HashIsSaved()
    	{
    		var directoryHash = new DirectoryHash(location);
			directoryHash.Save();
			Assert.That(File.Exists(Path.Combine(location, "directory.hash")));
    	}

    	[Test]
		public void HashIsValidatedIfNothingWasChanged()
		{
			var directoryHash = new DirectoryHash(location);
			directoryHash.Save();

			var directoryHash2 = new DirectoryHash(location);
			directoryHash2.Validate();
		}

		[Test]
    	public void HashValidationFailsIfDirectoryHashFileIsAbsent()
		{
			var directoryHash = new DirectoryHash(location);
			Assert.Throws<HashValidationException>(directoryHash.Validate);
		}

    	[Test]
    	public void HashValidationFailsIfSomeFileIsAbsent()
    	{
    		const string tempFile = "folder2/temp.file";
    		CreateFile(tempFile);
			var directoryHash = new DirectoryHash(location);
			directoryHash.Save();

			File.Delete(Path.Combine(location, tempFile));

			var directoryHash2 = new DirectoryHash(location);
			Assert.Throws<HashValidationException>(directoryHash2.Validate);
    	}

		[Test]
    	public void HashValidationFailsIfSomeFileIsChanged()
    	{
			CreateFile("folder2/temp.file");
			var directoryHash = new DirectoryHash(location);
			directoryHash.Save();

			// Recreate file with different contents
			CreateFile("folder2/temp.file"); 
			var directoryHash2 = new DirectoryHash(location);
			Assert.Throws<HashValidationException>(directoryHash2.Validate);
    	}

		[Test]
    	public void HashValidationFailsIfWholeKeyHashIsAbsent()
    	{
			var directoryHash1 = new DirectoryHash(location);
			directoryHash1.Save();

			var sb = new StringBuilder();

			string directoryHashFile = Path.Combine(location, "directory.hash");
			using (var sr = new StreamReader(directoryHashFile))
			{
				while (!sr.EndOfStream)
				{
					string line = sr.ReadLine();
					if (line == null || !line.StartsWith(":::"))
					{
						sb.AppendLine(line);
					}
				}
			}

			using (var sw = new StreamWriter(directoryHashFile))
			{
				sw.Write(sb.ToString());
			}

			var directoryHash = new DirectoryHash(location);
			Assert.Throws<HashValidationException>(directoryHash.Validate);
    	}

		[Test]
    	public void HashValidationFailsIfSomeEntryIsAbsent()
    	{
			var directoryHash1 = new DirectoryHash(location);
			directoryHash1.Save();

			var sb = new StringBuilder();

			string directoryHashFile = Path.Combine(location, "directory.hash");
			using (var sr = new StreamReader(directoryHashFile))
			{
				var index = 0;
				while (!sr.EndOfStream)
				{
					string line = sr.ReadLine();
					if (index != 1)
					{
						sb.AppendLine(line);
					}
					index++;
				}
			}

			using (var sw = new StreamWriter(directoryHashFile))
			{
				sw.Write(sb.ToString());
			}

			var directoryHash = new DirectoryHash(location);
			Assert.Throws<HashValidationException>(directoryHash.Validate);
    	}

		[Test]
		public void HashValidationSuccedesWhenContentsChangesInFileWithChangingContents()
		{
			CreateFile("optional.contents");
			var directoryHash = new DirectoryHash(location, mandatoryFilesWithAnyContent: new[]{"optional.contents"});
			directoryHash.Save();

			CreateFile("optional.contents");
			var directoryHash2 = new DirectoryHash(location);
			Assert.DoesNotThrow(directoryHash2.Validate);
		}

		[Test]
		public void HashValidationSuccedesWhenFileWithChangingContentsAbsents()
		{
			CreateFile("optional.contents");
			var directoryHash = new DirectoryHash(location, mandatoryFilesWithAnyContent: new[] { "optional.contents" });
			directoryHash.Save();

			File.Delete(Path.Combine(location, "optional.contents"));
			var directoryHash2 = new DirectoryHash(location);
			Assert.Throws<HashValidationException>(directoryHash2.Validate);
		}



    }
}