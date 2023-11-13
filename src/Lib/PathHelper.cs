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

        public static string GetProjectDirectoryPath(GlobalDataContext context, Project project) 
        {
            return Path.Combine(context.GetProjectsDirectoryPath(), project.Id, "packages");
        }

        public static string GetZipDownloadDirectoryPath(GlobalDataContext context, Project project, LocalPackage package)
        {
            return Path.Combine(GetProjectDirectoryPath(context, project), package.Package.Id, $"{package.Package.Id}.zip");
        }

        public static string GetZipDownloadDirectoryTempPath(GlobalDataContext context, Project project, LocalPackage package)
        {
            return Path.Combine(GetProjectDirectoryPath(context, project), package.Package.Id, $"~{package.Package.Id}.zip");
        }

        public static string GetPackageDirectoryPath(GlobalDataContext context, Project project, LocalPackage package)
        {
            return Path.Combine(GetProjectDirectoryPath(context, project), package.Package.Id, "content");
        }

        public static string GetPackageDirectoryTempPath(GlobalDataContext context, Project project, LocalPackage package)
        {
            return Path.Combine(GetProjectDirectoryPath(context, project), $"~{package.Package.Id}");
        }

        public static string GetPackageDirectoryDeletePath(GlobalDataContext context, Project project, LocalPackage package)
        {
            return Path.Combine(GetProjectDirectoryPath(context, project), $"!{package.Package.Id}");
        }
    }
}