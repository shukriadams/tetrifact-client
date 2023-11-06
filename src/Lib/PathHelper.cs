using System;
using System.IO;
using System.Reflection;

namespace TetrifactClient
{
    public static class PathHelper
    {
        /// <summary>
        /// Gets apps core data directory, this is directory where preferences/settings/logs etc are always stored. 
        /// </summary>
        /// <returns></returns>
        public static string GetInternalDirectory() 
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetExecutingAssembly().GetName().Name);
        }

        public static string GetZipDownloadDirectoryPath(Preferences preferences, Project project, LocalPackage package)
        {
            return Path.Combine(preferences.ProjectsRootDirectory, project.Id, "zips", $"{package.Package.Id}.zip");
        }

        public static string GetZipDownloadDirectoryTempPath(Preferences preferences, Project project, LocalPackage package)
        {
            return Path.Combine(preferences.ProjectsRootDirectory, project.Id, "zips", $"~{package.Package.Id}.zip");
        }

        public static string GetPackageDirectoryPath(Preferences preferences, Project project, LocalPackage package)
        {
            return Path.Combine(preferences.ProjectsRootDirectory, project.Id, $"{package.Package.Id}");
        }

        public static string GetPackageDirectoryTempPath(Preferences preferences, Project project, LocalPackage package)
        {
            return Path.Combine(preferences.ProjectsRootDirectory, project.Id, $"~{package.Package.Id}");
        }

        public static string GetPackageDirectoryDeletePath(Preferences preferences, Project project, LocalPackage package)
        {
            return Path.Combine(preferences.ProjectsRootDirectory, project.Id, $"!{package.Package.Id}");
        }
    }
}