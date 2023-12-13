using System.IO;

namespace TetrifactClient
{
    public static class PathHelper
    {
        public static string Combine(string path1, string path2) 
        {
            path1 = path1.Replace("\\", "/");
            path2 = path2.Replace("\\", "/");
            return Path.Combine(path1, path2);
        }

        public static string GetLogsDirectory(GlobalDataContext context) 
        {
            return Path.Join(context.DataFolder, "logs");
        }

        public static string GetZipDownloadDirectoryPath(GlobalDataContext context, Project project, LocalPackage package)
        {
            return Path.Combine(context.ProjectsRootDirectory, project.Id, $"{package.Package.Id}.zip");
        }

        public static string GetZipDownloadDirectoryTempPath(GlobalDataContext context, Project project, LocalPackage package)
        {
            return Path.Combine(context.ProjectsRootDirectory, project.Id, $"~{package.Package.Id}.zip");
        }

        public static string GetPackageDirectoryPath(GlobalDataContext context, Project project, LocalPackage package)
        {
            return Path.Combine(context.ProjectsRootDirectory, project.Id, package.Package.Id);
        }

        public static string GetPackageContentDirectoryPath(GlobalDataContext context, Project project, LocalPackage package)
        {
            return Path.Combine(context.ProjectsRootDirectory, project.Id, package.Package.Id, "_");
        }

        public static string GetPackageDirectoryTempPath(GlobalDataContext context, Project project, LocalPackage package)
        {
            return Path.Combine(context.ProjectsRootDirectory, project.Id, package.Package.Id, $"~");
        }

        public static string GetPackageDirectoryDeletePath(GlobalDataContext context, Project project, LocalPackage package)
        {
            return Path.Combine(context.ProjectsRootDirectory, project.Id, package.Package.Id, $"!");
        }
    }
}