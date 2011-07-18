using MSBuild.ExtensionPack.Framework;
using Microsoft.Build.Framework;

namespace NDistribUnit.MSBuild.Extensions
{
    public class AssemblyVersion : AssemblyInfo
    {
        public string Action { get; set; }

        [Output]
        public string BuiltVersion { get; set; }

        public override bool Execute()
        {
            if (string.IsNullOrEmpty(Action))
                Action = "UpdateVersionAttribute";

            switch (Action)
            {
                case "UpdateVersionAttribute":
                    return base.Execute() && GetVersionFromAttribute();
                case "GetVersionFromAttribute":
                    return GetVersionFromAttribute();
                default:
                    Log.LogError(string.Format("Action {0} is not supported", Action));
                    return false;
            }
        }

        private bool GetVersionFromAttribute()
        {
            if (AssemblyInfoFiles.Length == 0)
            {
                Log.LogError("There are no assembly info files to read from");
                return false;
            }
                

            foreach (var file in AssemblyInfoFiles)
            {
                var wrapper = new AssemblyInfoWrapper(file.ItemSpec);
                var version = wrapper["AssemblyVersion"];
                if (!string.IsNullOrEmpty(version))
                {
                    BuiltVersion = version;
                    return true;
                }
            }

            Log.LogError("Could not find assembly version in any assembly info file");
            return false;
        }
    }
}
