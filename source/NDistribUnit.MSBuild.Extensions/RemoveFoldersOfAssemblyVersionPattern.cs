using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Build.Utilities;
using System.Linq;

namespace NDistribUnit.MSBuild.Extensions
{
    public class RemoveFoldersOfAssemblyVersionPattern: Task
    {
        public static Regex VersionPattern = new Regex(@"^(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+)\.(?<revision>\d+)$", RegexOptions.Compiled);

        public string Path { get; set; }

        public int FoldersCountToLeave { get; set; }

        public override bool Execute()
        {
            var folder = new DirectoryInfo(Path);
            if (!folder.Exists)
                return true;

            var folders = folder.GetDirectories()
                .Where(f => VersionPattern.IsMatch(f.Name))
                .OrderBy(f => new Version(f.Name))
                .ToArray();

            for (int i = 0; i < folders.Length - FoldersCountToLeave; i++)
            {
                Log.LogMessage(string.Format("Removing folder: {0}", folders[i].Name));
                try
                {
                    folders[i].Delete(true);
                }
                catch(Exception ex)
                {
                    Log.LogWarningFromException(ex);
                }
            }

            return true;
        }
    }
}