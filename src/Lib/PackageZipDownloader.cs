using System;
using System.IO;

namespace TetrifactClient
{
    public class PackageZipDownloader : IPackageDownloader
    {
        #region FIELDS

        private ILog _log;

        private LocalPackage _package;

        private GlobalDataContext _context;

        private Project _project;

        #endregion

        #region PROPERTIES

        public IsTrueLookup CancelCheck { get; set; }

        #endregion

        #region CTORS

        public PackageZipDownloader(GlobalDataContext context, Project project, LocalPackage package, ILog log)
        {
            _package = package;
            _log = log;
            _context = context;
            _project = project;
        }

        #endregion

        #region METHODS

        public PackageTransferResponse Download()
        {
            try
            {
                HardenedWebClient webClient = new HardenedWebClient();
                webClient.Timeout = GlobalDataContext.Instance.Timeout;
                string zipFileTempPath = PathHelper.GetZipDownloadDirectoryTempPath(_context, _project, _package);
                string zipFilePath = PathHelper.GetZipDownloadDirectoryPath(_context, _project, _package);
                string finalPackagePath = PathHelper.GetPackageContentDirectoryPath(_context, _project, _package);
                string remoteZipUrl = $"{_package.TetrifactServerAddress}/v1/archives/{_package.Package.Id}";

                if (Directory.Exists(finalPackagePath)) 
                    return new PackageTransferResponse { Succeeded = true, Message = "Already downloaded" };

                _package.DownloadProgress.Message = "Contacting server ...";

                if (_package.TransferState == PackageTransferStates.UserCancellingDownload)
                    return new PackageTransferResponse { Succeeded = true, Message = "Cancelled" };

                // download zip if the zip's eventual location doesn't exist yet, note, this assumes that the existing zip isn't corrupted or broken from previous download attempt
                if (!File.Exists(zipFilePath))
                {
                    ChunkedDownloader downloader = new ChunkedDownloader();
                    downloader.OnChunkDownloaded += (file, n, total, perc) => { 
                        _package.DownloadProgress.Message = $"Downloaded {perc}%"; 
                    };
                    downloader.CancelCheck = () => this.CancelCheck();
                    downloader.OnChunkAssembled += (file, n, total, perc) => { _package.DownloadProgress.Message = $"Copied {perc}%"; };
                    downloader.OnError += ex => { _package.DownloadProgress.Message = "Download failed - check logs"; };
                    downloader.Download(remoteZipUrl,
                        zipFileTempPath,
                        _context.Preferences.DownloadChunkSize,
                        _context.Preferences.ParallelDownloadThreads);

                    if (downloader.Succeeded)
                    {
                        _package.DownloadProgress.Message = "Moving file";
                        File.Move(zipFileTempPath, zipFilePath);
                    }
                    else 
                    {
                        return new PackageTransferResponse { Succeeded = false, Message = "Download failed, check logs" };
                    }
                }

                // unpack zip
                PackageUnzip unpacker = new PackageUnzip(_context, _project, _package, zipFilePath);
                unpacker.CancelCheck = () => this.CancelCheck();
                unpacker.Unpack();

                return new PackageTransferResponse { Succeeded = true };
            }
            catch (Exception ex)
            {
                return new PackageTransferResponse { Exception = ex };
            }
        }

        #endregion
    }
}
