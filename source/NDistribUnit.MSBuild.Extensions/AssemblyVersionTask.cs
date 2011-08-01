using System;
using MSBuild.ExtensionPack.Framework;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NDistribUnit.MSBuild.Extensions
{
	[LoadInSeparateAppDomainAttribute]
// ReSharper disable UnusedMember.Global
    public class AssemblyVersionTask : AppDomainIsolatedTask
// ReSharper restore UnusedMember.Global
    {
        public override bool Execute()
        {
            if (string.IsNullOrEmpty(Action))
                Action = "UpdateVersionAttribute";

            switch (Action)
            {
                case "UpdateVersionAttribute":
                    return new AssemblyInfo
                               {
                                   AssemblyBuildNumber = AssemblyBuildNumber,
                                   AssemblyBuildNumberFormat = AssemblyBuildNumberFormat,
                                   AssemblyBuildNumberType = AssemblyBuildNumberType,
                                   AssemblyCompany = AssemblyCompany,
                                   AssemblyConfiguration = AssemblyConfiguration,
                                   AssemblyCopyright= AssemblyCopyright,
                                   AssemblyCulture = AssemblyCulture,
                                   AssemblyDelaySign = AssemblyDelaySign,
                                   AssemblyDescription = AssemblyDescription,
                                   AssemblyFileBuildNumber = AssemblyFileBuildNumber,
                                   AssemblyFileBuildNumberFormat= AssemblyFileBuildNumberFormat,
                                   AssemblyFileBuildNumberType = AssemblyFileBuildNumberType,
                                   AssemblyFileMajorVersion = AssemblyFileMajorVersion,
                                   AssemblyFileMinorVersion = AssemblyFileMinorVersion,
                                   AssemblyFileRevision = AssemblyFileRevision,
                                   AssemblyFileRevisionFormat = AssemblyFileRevisionFormat,
                                   AssemblyFileRevisionType = AssemblyFileRevisionType,
                                   AssemblyFileVersion = AssemblyFileVersion,
                                   AssemblyGuid = AssemblyGuid,
                                   AssemblyIncludeSigningInformation = AssemblyIncludeSigningInformation,
                                   AssemblyInfoFiles= AssemblyInfoFiles,
                                   AssemblyKeyFile= AssemblyKeyFile,
                                   AssemblyKeyName = AssemblyKeyName,
                                   AssemblyMajorVersion = AssemblyMajorVersion,
                                   AssemblyMinorVersion = AssemblyMinorVersion,
                                   AssemblyProduct = AssemblyProduct,
                                   AssemblyRevision = AssemblyRevision,
                                   AssemblyRevisionFormat = AssemblyRevisionFormat,
                                   AssemblyRevisionType = AssemblyRevisionType,
                                   AssemblyTitle = AssemblyTitle,
                                   AssemblyTrademark = AssemblyTrademark,
                                   AssemblyVersion = AssemblyVersion,
                                   BuildEngine = BuildEngine,
                                   ComVisible = ComVisible,
                                   FirstDayOfWeek = FirstDayOfWeek,
                                   HostObject = HostObject,
                                   MaxAssemblyFileVersion = MaxAssemblyFileVersion,
                                   MaxAssemblyVersion = MaxAssemblyVersion,
                                   PaddingCount = PaddingCount,
                                   PaddingDigit = PaddingDigit,
                                   SkipVersioning = SkipVersioning,
                                   StartDate = StartDate,
                                   TextEncoding = TextEncoding,
                                   UpdateAssemblyInformationalVersion = UpdateAssemblyInformationalVersion,
                                   UseUtc = UseUtc ,
                               }.Execute() && GetVersionFromAttribute();
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

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
        public string Action { get; set; }
        [Output]
        public string BuiltVersion { get; set; }
        public string AssemblyBuildNumber { get; set; }
        public string AssemblyBuildNumberFormat { get; set; }
        public string AssemblyBuildNumberType { get; set; }
        public string AssemblyCompany { get; set; }
        public string AssemblyConfiguration { get; set; }
        public string AssemblyCopyright { get; set; }
        public string AssemblyCulture { get; set; }
        public string AssemblyDelaySign { get; set; }
        public string AssemblyDescription { get; set; }
        public string AssemblyFileBuildNumber { get; set; }
        public string AssemblyFileBuildNumberFormat { get; set; }
        public string AssemblyFileBuildNumberType { get; set; }
        public string AssemblyFileMajorVersion { get; set; }
        public string AssemblyFileMinorVersion { get; set; }
        public string AssemblyFileRevision { get; set; }
        public string AssemblyFileRevisionFormat { get; set; }
        public string AssemblyFileRevisionType { get; set; }
        public string AssemblyFileVersion { get; set; }
        public string AssemblyGuid { get; set; }
        public bool AssemblyIncludeSigningInformation { get; set; }
        public ITaskItem[] AssemblyInfoFiles { get; set; }
        public string AssemblyKeyFile { get; set; }
        public string AssemblyKeyName { get; set; }
        public string AssemblyMajorVersion { get; set; }
        public string AssemblyMinorVersion { get; set; }
        public string AssemblyProduct { get; set; }
        public string AssemblyRevision { get; set; }
        public string AssemblyRevisionFormat { get; set; }
        public string AssemblyRevisionType { get; set; }
        public string AssemblyTitle { get; set; }
        public string AssemblyTrademark { get; set; }
        public string AssemblyVersion { get; set; }
        public string ComVisible { get; set; }
        public string FirstDayOfWeek { get; set; }
        [Output]
        public string MaxAssemblyFileVersion { get; set; }
        [Output]
        public string MaxAssemblyVersion { get; set; }
        public int PaddingCount { get; set; }
        public char PaddingDigit { get; set; }
        public bool SkipVersioning { get; set; }
        public DateTime StartDate { get; set; }
        public string TextEncoding { get; set; }
        public bool UpdateAssemblyInformationalVersion { get; set; }
        public bool UseUtc { get;set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore MemberCanBePrivate.Global
    }
}
