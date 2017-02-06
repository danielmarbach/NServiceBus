namespace NServiceBus
{
    using System;
    using System.Reflection;

    static class GitFlowVersion
    {
        static GitFlowVersion()
        {
            var assembly = typeof(GitFlowVersion).GetTypeInfo().Assembly;
            var gitFlowVersionInformationType = assembly.GetType("NServiceBus.GitVersionInformation", true);
            var fieldInfo = gitFlowVersionInformationType.GetTypeInfo().GetField("MajorMinorPatch");
            var majorMinorPatchVersion = Version.Parse((string) fieldInfo.GetValue(null));
            MajorMinor = majorMinorPatchVersion.ToString(2);
            MajorMinorPatch = majorMinorPatchVersion.ToString(3);
        }

        public static string MajorMinor;
        public static string MajorMinorPatch;
    }
}