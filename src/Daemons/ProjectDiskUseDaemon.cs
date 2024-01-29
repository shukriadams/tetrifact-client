using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TetrifactClient
{
    public class ProjectDiskUseDaemon : IDaemon
    {
        private ILog _log;

        public void Start()
        {
            DaemonProcessRunner runner = new DaemonProcessRunner();
            _log = new Log();
            // run at 5 minute timer because this doesn't need to be instantaneously updated
            runner.Start(new AsyncDo(this.Work), (int)new TimeSpan(0, 0, 10).TotalMilliseconds, new Log());
        }

        public void WorkNow()
        {
            throw new NotImplementedException();
        }

        public async Task Work()
        {
            foreach (Project project in GlobalDataContext.Instance.Projects.Projects)
            {
                long totalPackageSize = 0;
                int invalidPackageDirectories = 0;
                string projectRootDir = Path.Join(GlobalDataContext.Instance.ProjectsRootDirectory, project.Id);
                string[] directories = Directory.GetDirectories(projectRootDir);
                directories = directories.Select(d => Path.GetFileName(d)).ToArray();

                foreach (LocalPackage package in project.Packages)
                {
                    string projectDataFile = Path.Join(GlobalDataContext.Instance.ProjectsRootDirectory, project.Id, package.Package.Id, "remote.json");
                    string downloadedContentDir = Path.Join(GlobalDataContext.Instance.ProjectsRootDirectory, project.Id, package.Package.Id, "_");

                    if (!File.Exists(projectDataFile))
                        continue;

                    if (Directory.Exists(downloadedContentDir))
                    {
                        totalPackageSize += package.Package.Size;
                    }
                    else 
                    {
                        if (!directories.Contains(package.Package.Id))
                        {
                            // this almost never happens, not sure if necessary
                            invalidPackageDirectories++;
                        }
                    }
                }

                if (totalPackageSize == 0)
                {
                    project.DiskUse = string.Empty;
                }
                else 
                {
                    project.DiskUse = $"Disk use : {FileSystemHelper.BytesToMegabytes(totalPackageSize).ToString("#,##0")} gb";
                    if (invalidPackageDirectories > 0)
                        project.DiskUse += $"{invalidPackageDirectories} invalid packages";
                }
            }
        }
    }
}
