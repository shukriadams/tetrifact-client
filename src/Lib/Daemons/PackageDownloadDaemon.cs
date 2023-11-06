using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TetrifactClient
{
    public class PackageDownloadDaemon : IDaemon
    {
        ILog _log;
        public void Start()
        {
            DaemonProcessRunner runner = new DaemonProcessRunner();
            _log = new Log();
            runner.Start(new AsyncDo(this.Work), GlobalDataContext.Instance.DaemonIntervalMS, _log);
        }

        public async Task Work()
        {
            Preferences preferences = 

            foreach (Project project in GlobalDataContext.Instance.Projects.Projects)
            {
                foreach (LocalPackage package in project.Packages.Where(package => package.IsQueuedForDownload()))
                {
                    await this.ProcessPackage(project, package);
                }
            }
        }

        private async Task ProcessPackage(Project project, LocalPackage package)
        {
            // find out if package download should be partial or full. Full is needed if no other package is available locally,
            // or if diff between this package and previous one is over a % of total files in build.
            //bool packageAlreadyDownlaoded = project.

            //find closest build
            IEnumerable<LocalPackage> allPackagesFromTargetServer = GlobalDataContext.Instance.Projects.Projects
                .Where(project => project.TetrifactServerAddress == package.TetrifactServerAddress)
                .Select(project => project.Packages.Where(package => package.IsExecutable()))
                .SelectMany(p => p)
                .OrderByDescending(p => p.Package.CreatedUtc);

            LocalPackage packageAfter = allPackagesFromTargetServer.FirstOrDefault(p => p.Package.CreatedUtc > package.Package.CreatedUtc);
            LocalPackage packageBefore = allPackagesFromTargetServer.FirstOrDefault(p => p.Package.CreatedUtc < package.Package.CreatedUtc);
            LocalPackage donorPackage = null;

            if (packageAfter != null && packageBefore == null)
                donorPackage = packageAfter;
            else if (packageAfter == null && packageBefore != null)
                donorPackage = packageBefore;
            else
            {
                if ((packageAfter.Package.CreatedUtc - package.Package.CreatedUtc).TotalMilliseconds > (packageBefore.Package.CreatedUtc - package.Package.CreatedUtc).TotalMilliseconds)
                    donorPackage = packageBefore;
                else
                    donorPackage = packageAfter;
            }

            PackageDownloader downloader = new PackageDownloader(preferences, project, package, _log);

            if (donorPackage == null)
            {
                // no donor package, download a fill zip
            }
            else
            {
                // partial zip
            }
        }
    }
}
