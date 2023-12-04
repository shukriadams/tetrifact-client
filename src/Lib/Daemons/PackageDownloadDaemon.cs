using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TetrifactClient
{
    public class PackageDownloadDaemon : IDaemon
    {
        #region FIELDS

        private ILog _log;

        private GlobalDataContext _context;

        private DaemonProcessRunner _runner;

        #endregion

        #region CTORS

        public PackageDownloadDaemon() 
        {
            // todo : we need to get rid of this static ref
            _context = GlobalDataContext.Instance;
            _runner = new DaemonProcessRunner();
            _log = new Log();
        }

        #endregion

        #region METHODS

        public void Start()
        {
            _runner.Start(new AsyncDo(this.Work), GlobalDataContext.Instance.DaemonIntervalMS, _log);
        }

        public void WorkNow() 
        {
            if (_runner != null)
                _runner.WorkNow();
        }

        private async Task Work()
        {
            System.Diagnostics.Debug.WriteLine($"PackageDownloadDaemon:WORK {DateTime.Now.Second}");
            foreach (Project project in GlobalDataContext.Instance.Projects.Projects) 
            {
                IEnumerable<LocalPackage> packagesToSync = project.Packages
                    .Where(package => package.IsQueuedForDownload());

                foreach (LocalPackage package in packagesToSync)
                    await this.ProcessPackage(project, package);
            }
        }

        private async Task ProcessPackage(Project project, LocalPackage package)
        {
            // find out if package download should be partial or full. Full is needed if no other package is available locally,
            // or if diff between this package and previous one is over a % of total files in build.
            //bool packageAlreadyDownlaoded = project.
            Preferences preferences = GlobalDataContext.Instance.Preferences;
            package.Status = "Processing...";

            //find closest build
            IEnumerable<LocalPackage> allPackagesFromTargetServer = GlobalDataContext.Instance.Projects.Projects
                .Where(project => project.TetrifactServerAddress == package.TetrifactServerAddress)
                .Select(project => project.Packages.Where(package => package.IsExecutable()))
                .SelectMany(p => p)
                .OrderByDescending(p => p.Package.CreatedUtc);

            PackageDiff packageDiff = null;
            LocalPackage donorPackage = null;
            if (allPackagesFromTargetServer.Any()) 
            {
                package.Status = "Checking local files first";
                foreach (LocalPackage potentialDonorPackage in allPackagesFromTargetServer)
                {
                    PackageDiffResponse diffLookup = JsonHelper.DownloadDiff(package.TetrifactServerAddress, potentialDonorPackage.Package.Id, package.Package.Id);
                    if (diffLookup.ResponseType == PackageDiffResponseTypes.None) 
                    {
                        packageDiff = diffLookup.PackageDiff;
                        donorPackage = potentialDonorPackage;
                        break;
                    }
                }
            }

            IPackageDownloader downloader = null;
            if (packageDiff == null)
                downloader = new PackageZipDownloader(_context, project, package, _log);
            else
                downloader = new PackagePartialDownloader(_context, project, package, donorPackage, packageDiff, _log);

            downloader.CancelCheck =()=> package.TransferState == PackageTransferStates.UserCancellingDownload;

            package.Status = "Downloading";
            package.TransferState = PackageTransferStates.Downloading;
            PackageTransferResponse result = downloader.Download();

            if (package.TransferState == PackageTransferStates.UserCancellingDownload)
            {
                package.TransferState = PackageTransferStates.DownloadCancelled;
                package.Status = "Cancelled";
                return;
            }

            if (result.Succeeded)
            {
                package.TransferState = PackageTransferStates.Downloaded;
                package.Status = "Done";
            }
            else 
            {
                package.TransferState = PackageTransferStates.DownloadFailed;
                package.Status = "Error, check logs";
                GlobalDataContext.Instance.Console.Add(result.Message);
                _log.LogError(result.Exception, result.Message);
            }
        }

        #endregion
    }
}
