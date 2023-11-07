﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TetrifactClient
{
    public class PackageDownloadDaemon : IDaemon
    {
        #region FIELDS

        private ILog _log;

        private IPreferencesProvider _preferencesProvider;

        #endregion
        
        #region CTORS

        public PackageDownloadDaemon(IPreferencesProvider preferencesProvider) 
        {
            _preferencesProvider = preferencesProvider;
        }

        #endregion

        #region METHODS

        public void Start()
        {
            DaemonProcessRunner runner = new DaemonProcessRunner();
            _log = new Log();
            runner.Start(new AsyncDo(this.Work), GlobalDataContext.Instance.DaemonIntervalMS, _log);
        }

        public async Task Work()
        {
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
            Preferences preferences = _preferencesProvider.GetInstance();

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
                foreach(LocalPackage potentialDonorPackage in allPackagesFromTargetServer)
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
                downloader = new PackageZipDownloader(preferences, project, package, _log);
            else
                downloader = new PackagePartialDownloader(preferences, project, package, donorPackage, packageDiff, _log);

            PackageTransferResponse result = downloader.Download();
            if (result.Succeeded)
            {
                // ?
            }
            else 
            {
                GlobalDataContext.Instance.Console.Add(result.Message);
            }
        }

        #endregion
    }
}