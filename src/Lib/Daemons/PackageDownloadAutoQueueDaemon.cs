using System;
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

                int downloadedCount = 0;
                // Work way down packages in order of listing, marking them for autodownload as necessary
                // This assumes packages are sorted newest to oldest
                foreach (LocalPackage package in project.Packages.Items) 
                {
                    // mark for download
                    if (package.IsEligibleForAutoDownload())
                        package.TransferState = BuildTransferStates.AutoMarkedForDownload;

                    // if alreaddy marked for download, tally it up
                    if (package.IsDownloadedorQueuedForDownload())
                        downloadedCount ++;

                    // if tally of (marked for) downloaded mnet, start marking for delete
                    if (downloadedCount > project.PackageSyncCount && package.CanBeAutoCleanedUp())
                        package.TransferState = BuildTransferStates.AutoMarkedForDelete;
                }
            }
        }
    }
}
