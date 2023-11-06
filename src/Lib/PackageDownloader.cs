using System;
using System.IO;

namespace TetrifactClient
{
    public class PackageDownloader
    {
        #region FIELDS

        private ILog _log;

        private LocalPackage _package;

        private Preferences _preferences;

        private Project _project;

        #endregion

        #region CTORS

        public PackageDownloader(Preferences preferences, Project project, LocalPackage package, ILog log)
        {
            _package = package;
            _log = log;
            _preferences = preferences;
            _project = project;
        }

        #endregion

        #region METHODS

        public void DownloadAsZip()
        {
            try
            {
                HardenedWebClient webClient = new HardenedWebClient();
                string zipFileTempPath = PathHelper.GetZipDownloadDirectoryTempPath(_preferences, _project, _package);
                string zipFilePath = PathHelper.GetZipDownloadDirectoryPath(_preferences, _project, _package);
                string finalPackagePath = PathHelper.GetPackageDirectoryPath(_preferences, _project, _package);
                string remoteZipUrl = $"{_package.TetrifactServerAddress}/v1/archives/{_package.Package.Id}";

                if (Directory.Exists(finalPackagePath)) 
                {
                    return;
                }

                PackageTransferProgress progress = PackageTransferProgressStore.Get(_project, _package);
                progress.Message = "Contacting server ...";

                // download zip if the zip's eventual location doesn't exist yet, note, this assumes that the existing zip isn't corrupted or broken from previous download attempt
                if (!File.Exists(zipFilePath))
                {
                    ChunkedDownloader downloader = new ChunkedDownloader();
                    downloader.OnChunkDownloaded += (file, n, total, perc) => { progress.Message = $"Downloaded {perc}%"; };
                    downloader.OnChunkAssembled += (file, n, total, perc) => { progress.Message = $"Copied {perc}%"; };
                    downloader.OnError += ex => { progress.Message = "Download failed - check logs"; };
                    downloader.Download(remoteZipUrl,
                        zipFileTempPath,
                        _preferences.DownloadChunkSize,
                        _preferences.ParallelDownloadThreads);

                    if (downloader.Succeeded) 
                    {
                        progress.Message = "Moving file";
                        File.Move(zipFileTempPath, zipFilePath);
                    }
                }

                // unpack zip
                PackageUnzip unpacker = new PackageUnzip(_preferences, _project, _package, zipFilePath);
                unpacker.Unpack();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Failed to download full build {_package.Package.Id}");
                throw;
            }
        }

        public void DownloadPartial() 
        {

        }

        #endregion
    }
}
