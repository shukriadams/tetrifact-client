using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TetrifactClient
{
    /// <summary>
    /// Resets cancelled packages back to available if applicable. This logic is kept in its own daemon to prevent cluttering up the normal
    /// package metadata download daemons with an additional pathway.
    /// </summary>
    public class CancelledPackageResetDaemon : IDaemon
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
                {
                    if (project.AvailablePackageIds.Contains(cancelledPackage.Package.Id))
                        cancelledPackage.TransferState = PackageTransferStates.AvailableForDownload;
                }
            }
        }
    }
}
