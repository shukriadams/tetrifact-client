﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TetrifactClient
{
    /// <summary>
    /// Resets cancelled and deleted packages available if applicable. This logic is kept in its own daemon to prevent cluttering up the normal
    /// package metadata download daemons with an additional pathway.
    /// </summary>
    public class PackageReturnToAvailablePoolDaemon : IDaemon
    {
        private ILog _log;

        public void Start()
        {
            DaemonProcessRunner runner = new DaemonProcessRunner();
            _log = new Log();
            runner.Start(new AsyncDo(this.Work), (int)new TimeSpan(0, 0, 5).TotalMilliseconds, new Log());
        }

        public void WorkNow()
        {
            throw new NotImplementedException();
        }

        public async Task Work()
        {
            GlobalDataContext context = GlobalDataContext.Instance;

            foreach (Project project in GlobalDataContext.Instance.Projects.Projects)
            {
                IEnumerable<LocalPackage> cancelledPackages = project.Packages.Where(p => p.TransferState == PackageTransferStates.DownloadCancelled);
                foreach (LocalPackage cancelledPackage in cancelledPackages)
                    if (project.AvailablePackageIds.Contains(cancelledPackage.Package.Id))
                        cancelledPackage.TransferState = PackageTransferStates.AvailableForDownload;

                // if a package is marked as usercancelled but isn't downloading, app restarted while cancelling. marked as cancelled
                IEnumerable<LocalPackage> cancellingPackages = project.Packages.Where(p => p.TransferState == PackageTransferStates.UserCancellingDownload);
                foreach (LocalPackage cancellingPackage in cancellingPackages)
                    if (!cancellingPackage.IsDownloading)
                        cancellingPackage.TransferState = PackageTransferStates.DownloadCancelled;

                IEnumerable<LocalPackage> deletedPackages = project.Packages.Where(p => p.TransferState == PackageTransferStates.Deleted);
                foreach (LocalPackage deletedPackage in deletedPackages)
                    if (project.AvailablePackageIds.Contains(deletedPackage.Package.Id))
                        deletedPackage.TransferState = PackageTransferStates.AvailableForDownload;

            }
        }
    }
}