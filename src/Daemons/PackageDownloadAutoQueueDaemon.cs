using System;
using System.Linq;
using System.Threading.Tasks;

namespace TetrifactClient
{
    /// <summary>
    /// Automatically queues packages to be downloaded. Does not
    /// </summary>
    public class PackageDownloadAutoQueueDaemon : IDaemon
    {
        private ILog _log;

        private bool _busy;

        private Preferences _preferences;

        private GlobalDataContext _globalContext;

        public PackageDownloadAutoQueueDaemon(ILog log, Preferences preferences, GlobalDataContext globalContext)
        {
            _preferences = preferences;
            _log = log;
            _globalContext = globalContext;
        }

        public void Start()
        {
            DaemonProcessRunner runner = new DaemonProcessRunner();
            Log log = new Log();
            runner.Start(new AsyncDo(this.Work), GlobalDataContext.Instance.DaemonIntervalMS, new Log());
        }

        public void WorkNow()
        {
            throw new NotImplementedException();
        }

        private async Task Work()
        {
            foreach (Project project in GlobalDataContext.Instance.Projects.Projects)
            {
                // no need to do anything if auto download disabled, user will mark packges for download themselves I guess.
                if (!project.AutoDownload)
                    continue;

                int downloadedCount = project.Packages.Where(p => p.IsExecutable()).Count();
                int aleadyQueued = project.Packages.Where(p => p.IsDownloadedorQueuedForDownload()).Count();
                int requiredToDownload = project.PackageSyncCount - downloadedCount - aleadyQueued;
                int queuedCount = 0;

                if (requiredToDownload > 0) 
                {
                    // Work way down packages in order of listing, marking them for autodownload as necessary
                    // This assumes packages are sorted newest to oldest
                    foreach (LocalPackage package in project.Packages)
                    {
                        // mark for download
                        if (package.IsEligibleForAutoDownload())
                        {
                            package.TransferState = PackageTransferStates.AutoMarkedForDownload;
                            queuedCount++;

                            project.SetStatus($"Queueing {package.Package.Id} for download");
                        }

                        if (queuedCount >= requiredToDownload)
                            break;
                    }
                }

                // mark for delete
                if (downloadedCount > project.PackageSyncCount) 
                {
                    int removeCount = 0;
                    foreach (LocalPackage package in project.Packages.Where(p => p.IsExecutable()).Reverse())
                    {
                        package.TransferState = PackageTransferStates.AutoMarkedForDelete;
                        removeCount ++;

                        project.SetStatus($"Marking {package.Package.Id} for delete");

                        if (downloadedCount >= project.PackageSyncCount + removeCount)
                            break;
                    }
                }
            }
        }
    }
}
