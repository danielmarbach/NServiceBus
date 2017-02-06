namespace NServiceBus
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Helper class to retrieve File version.
    /// </summary>
    class FileVersionRetriever
    {
        /// <summary>
        /// Retrieves a semver compliant version from a <see cref="Type" />.
        /// </summary>
        /// <param name="type"><see cref="Type" /> to retrieve version from.</param>
        /// <returns>SemVer compliant version.</returns>
        public static string GetFileVersion(Type type)
        {
            var customAttributes = type.GetTypeInfo().Assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute));

            var fileVersion = (AssemblyFileVersionAttribute)customAttributes.ElementAtOrDefault(0);
            Version version;
            if (Version.TryParse(fileVersion.Version, out version))
            {
                return version.ToString(3);
            }

            return type.GetTypeInfo().Assembly.GetName().Version.ToString(3);
        }
    }
}