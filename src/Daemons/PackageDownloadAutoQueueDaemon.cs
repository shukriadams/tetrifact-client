using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TetrifactClient
{
    /// <summary>
    /// Automatically queues packages to be downloaded, and to be deleted. Does not not the actual download/delete, only queuing.  
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
            // no need to do anything if auto download disabled, user will mark packges for download themselves
            foreach (Project project in GlobalDataContext.Instance.Projects.Projects.Where(p => p.AutoDownload))
            {
                int downloadedCount = project.Packages.Where(p => p.IsExecutable()).Count();
                int aleadyQueued = project.Packages.Where(p => p.IsDownloadedOrQueuedForDownload()).Count();
                int requiredToDownload = project.PackageSyncCount - downloadedCount - aleadyQueued;
                int queuedCount = 0;

                if (requiredToDownload > 0) 
                {
                    // Work way down packages in order of listing, marking them for autodownload as necessary
                    // This assumes packages are sorted newest to oldest
                    for (int i = 0 ;  i < project.Packages.Count; i ++)
                    {
                        LocalPackage package = project.Packages[i];

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

                // mark older packages for delete, only if they're executable and have been autodownloaded
                if (downloadedCount > project.PackageSyncCount) 
                {
                    int removeCount = 0;
                    IList<LocalPackage> cloned = project.Packages.Where(p => p.IsAttemptedDownloaded() && p.WasAutoDownloaded).Reverse().ToList();

                    for (int i = 0; i < cloned.Count; i ++)
                    {
                        LocalPackage package = cloned[i];
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
