using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TetrifactClient
{
    public class PackagePartialDownloader : IPackageDownloader
    {
        #region FIELDS

        private ILog _log;

        private LocalPackage _package;

        private Preferences _preferences;

        private GlobalDataContext _dataContext;

        private Project _project;

        private LocalPackage _donorPackage;

        PackageDiff _packageDiff;

        #endregion

        #region CTORS

        public PackagePartialDownloader(GlobalDataContext dataContext, Project project, LocalPackage package, LocalPackage donorPackage, PackageDiff packageDiff, ILog log)
        {
            _package = package;
            _donorPackage = donorPackage;
            _log = log;
            _dataContext = dataContext;
            _project = project;
            _packageDiff = packageDiff;
        }

        #endregion

        #region METHODS

        public PackageTransferResponse Download() 
        {
            PackageTransferProgress progress = PackageTransferProgressStore.Get(_project, _package);
            string finalPackagePath = PathHelper.GetPackageDirectoryPath(_dataContext, _project, _package);

            // Download new files
            if (_packageDiff.Difference.Any()) 
            {
                int downloadCount = 0;
                PackageTransferResponse downloadErrorResponse = null;
                foreach (PackageFile missingFile in _packageDiff.Difference)
                    try
                    {
                        string savePath = Path.Combine(finalPackagePath, missingFile.Path);
                        if (savePath.Length > Constants.MAX_PATH_LENGTH)
                            return new PackageTransferResponse
                            {
                                Result = PackageTransferResultTypes.FilePathTooLong,
                                Message = $"Path {savePath}"
                            };

                        FileSystemHelper.CreateDirectory(Path.GetDirectoryName(savePath));

                        ChunkedDownloader downloader = new ChunkedDownloader();
                        downloader.OnError += (ex) => {
                            downloadErrorResponse = new PackageTransferResponse 
                            {
                                Exception = ex,
                                Result = PackageTransferResultTypes.FileDownloadFailed,
                                Message = "Download failed"
                            };
                        };

                        downloadCount ++;
                        progress.Message = $"Downloading {MathHelper.Percent(downloadCount, _packageDiff.Difference.Count)}%";
                        downloader.Download($"{_package.TetrifactServerAddress}/v1/files/{missingFile.Id}", savePath, 1000000, 1); // 1 meg chunk size, use 1 thread only, as this process is already threaded!

                        // error occured, exit download process immediately
                        if (downloadErrorResponse != null)
                            return downloadErrorResponse;
                    }
                    catch (Exception ex)
                    {
                        return new PackageTransferResponse
                        {
                            Exception = ex,
                            Result = PackageTransferResultTypes.FileDownloadFailed,
                            Message = $"Failed to download file {missingFile.Path}"
                        };
                    }
            }

            // copy local files from donor build
            int copiedCount = 0;

            int countall = _packageDiff.Common.Count();

            List<PackageFile> copyFails = new List<PackageFile>();
            int parallels = 4;

            string donorBuildPath = PathHelper.GetPackageDirectoryPath(_dataContext, _project, _donorPackage);

            // MaxDegreeOfParallellism needs tweaking to find optimum without overloading disk
            PackageTransferResponse copyErrorResponse = null;
            Parallel.ForEach(_packageDiff.Common.Select(fn => fn), new ParallelOptions { MaxDegreeOfParallelism = parallels }, existingFile => {
                progress.Message = $"Copying {MathHelper.Percent(copiedCount, countall)}%";

                string newFilePath = Path.Combine(finalPackagePath, existingFile.Path);
                if (newFilePath.Length > Constants.MAX_PATH_LENGTH)
                    copyErrorResponse =  new PackageTransferResponse
                    {
                        Result = PackageTransferResultTypes.FilePathTooLong,
                        Message = $"Path {newFilePath}"
                    };

                FileSystemHelper.CreateDirectory(Path.GetDirectoryName(newFilePath));

                string donorFilePath = Path.Combine(donorBuildPath, existingFile.Path);

                // check and copy file, if that returns false, copy failed, we need to directly download file
                bool fileCopied = VerifyAndCopyFile(donorFilePath, newFilePath, existingFile.Hash, _package.Package.Id);

                if (fileCopied)
                {
                    _log.LogDebug($"Copied local file {copiedCount} of {countall}, {existingFile.Path}");
                }
                else
                {
                    lock (copyFails)
                    {
                        copyFails.Add(existingFile);
                    }
                    _log.LogDebug($"Error copying local file {copiedCount} of {countall}, {existingFile.Path}");
                }

                copiedCount++; // Include failed files
            });

            if (copyFails.Any())
                return new PackageTransferResponse 
                {
                    Result = PackageTransferResultTypes.LocalCopyFail,
                    Message = $"Files : {string.Join(", ", copyFails)}"
                
                };

            // write state to package
            _package.TransferState = PackageTransferStates.Downloaded;
            // todo : log more info about transfer time etc?

            return new PackageTransferResponse { };
        }

        private bool VerifyAndCopyFile(string donorFilePath, string newFilePath, string donorSHA256, string packageId)
        {
            // Check donor file exists
            if (!File.Exists(donorFilePath))
            {
                _log.LogDebug($"Server common list error or doner file missing - expected file {donorFilePath} not found - downloading instead");
                return false;
            }

            // Check CRC of donor file
            string fileSHA256;
            try
            {
                fileSHA256 = HashLib.FromFile(donorFilePath);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Unable to calculate SHA256 for {donorFilePath}");
                return false;
            }

            if (fileSHA256 != donorSHA256)
            {
                _log.LogDebug($"Wrong checksum for file {donorFilePath} - downloading instead, fileSHA256:{fileSHA256}, donorSHA256:{donorSHA256}, packageId:{packageId}");
                return false;
            }

            // All fine - try to copy file
            try
            {
                File.Copy(donorFilePath, newFilePath, true);
            }
            catch (IOException ex)
            {
                _log.LogError(ex, $"Copy failed {donorFilePath} - downloading instead");
                return false;
            }

            return true;
        }

        #endregion
    }
}
