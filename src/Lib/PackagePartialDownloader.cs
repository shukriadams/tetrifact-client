using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TetrifactClient
{
    public class PackagePartialDownloader : IPackageDownloader, ICancel
    {
        #region FIELDS

        private ILog _log;

        private LocalPackage _package;

        private GlobalDataContext _dataContext;

        private Project _project;

        private LocalPackage _donorPackage;

        PackageDiff _packageDiff;

        #endregion

        #region PROPERTIES

        public IsTrueLookup CancelCheck { get; set; }

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
            string finalPackagePath = PathHelper.GetPackageContentDirectoryPath(_dataContext, _project, _package);

            IList<PackageFile> filesToDownload = new PackageFile[0];
            int countall = 0;

            if (_packageDiff != null)
            {
                countall = _packageDiff.Common.Count();
                filesToDownload = _packageDiff.Difference;  
            }
            else
            {
                // load files from disk
                string packageFilesListPath = Path.Combine(GlobalDataContext.Instance.ProjectsRootDirectory, _project.Id, _package.Package.Id, "files.json");
                JsonFileLoadResponse<IEnumerable<PackageFile>> jsonLoadResponse = JsonHelper.LoadJSONFile<IEnumerable<PackageFile>>(packageFilesListPath, true, true);
                if (jsonLoadResponse.Payload == null)
                    throw new Exception($"Failed to load full Package {_package.Package.Id} in project {_project.Name}, error {jsonLoadResponse.ErrorType}.", jsonLoadResponse.Exception);

                filesToDownload = jsonLoadResponse.Payload.ToList();
                countall = filesToDownload.Count();
            }

            // copy local files from donor build
            int copiedCount = 0;

            List<PackageFile> copyFails = new List<PackageFile>();

            if (_packageDiff != null && _donorPackage != null)
            {
                string donorBuildPath = PathHelper.GetPackageContentDirectoryPath(_dataContext, _project, _donorPackage);

                // MaxDegreeOfParallellism needs tweaking to find optimum without overloading disk
                PackageTransferResponse copyErrorResponse = null;
                Parallel.ForEach(_packageDiff.Common.Select(fn => fn), new ParallelOptions { MaxDegreeOfParallelism = GlobalDataContext.Instance.ThreadLoad }, existingFile => {

                    if (_package.TransferState == PackageTransferStates.UserCancellingDownload)
                        return;

                    _package.DownloadProgress.Message = $"Copying {MathHelper.Percent(copiedCount, countall)}% ({copiedCount}/{countall})";

                    string newFilePath = Path.Combine(finalPackagePath, existingFile.Path);
                    if (newFilePath.Length > Constants.MAX_PATH_LENGTH)
                        copyErrorResponse = new PackageTransferResponse
                        {
                            Result = PackageTransferResultTypes.FilePathTooLong,
                            Message = $"Path {newFilePath}"
                        };

                    FileSystemHelper.CreateDirectory(Path.GetDirectoryName(newFilePath));

                    string donorFilePath = Path.Combine(donorBuildPath, existingFile.Path);

                    // check and copy file, if that returns false, copy failed, we need to directly download file
                    bool fileCopied = VerifyAndCopyFile(donorFilePath, newFilePath, existingFile.Hash, _package.Package.Id);

                    if (!fileCopied)
                    {
                        filesToDownload.Add(existingFile);
                    }

                    copiedCount++; // Include failed files
                });

                if (copyFails.Any())
                    return new PackageTransferResponse
                    {
                        Result = PackageTransferResultTypes.LocalCopyFail,
                        Message = $"Files : {string.Join(", ", copyFails)}"
                    };
            }

            // Download new files
            if (filesToDownload.Any()) 
            {
                // check if download file paths are too long
                foreach (PackageFile fileToDownload in filesToDownload) 
                {
                    string savePath = Path.Combine(finalPackagePath, fileToDownload.Path);
                    if (savePath.Length > Constants.MAX_PATH_LENGTH)
                        return new PackageTransferResponse
                        {
                            Result = PackageTransferResultTypes.FilePathTooLong,
                            Message = $"Path {savePath} is too long to save to disk. Try moving save path to a location closer to root of disk."
                        };
                }

                int downloadCount = 0;
                int totalDownloadCount = filesToDownload.Count();
                PackageTransferResponse downloadErrorResponse = null;

                Parallel.ForEach(filesToDownload.Select(fn => fn), new ParallelOptions { MaxDegreeOfParallelism = GlobalDataContext.Instance.ThreadLoad }, missingFile =>
                {
                    try
                    {
                        // error occurred by a sibling process, exit download process immediately
                        if (downloadErrorResponse != null)
                            return;

                        // break out of look if cancel set from parent process
                        if (this.CancelCheck != null && this.CancelCheck())
                            return;

                        // NOTE : Path combine returns only path2 if slashes mismatch
                        string savePath = Path.Join(finalPackagePath, missingFile.Path);
                        FileSystemHelper.CreateDirectory(Path.GetDirectoryName(savePath));

                        ChunkedDownloader downloader = new ChunkedDownloader();
                        downloader.CancelCheck = () => this.CancelCheck();
                        downloader.OnError += (ex) => {
                            downloadErrorResponse = new PackageTransferResponse
                            {
                                Exception = ex,
                                Result = PackageTransferResultTypes.FileDownloadFailed,
                                Message = "Download failed"
                            };
                        };

                        downloadCount++;
                        _package.DownloadProgress.Message = $"Downloading {MathHelper.Percent(downloadCount, totalDownloadCount)}% ({downloadCount}/{totalDownloadCount})";
                        downloader.Download($"{_package.TetrifactServerAddress}/v1/files/{missingFile.Id}", savePath, 1000000, 1); // 1 meg chunk size, use 1 thread only, as this process is already threaded!

                        GlobalDataContext.Instance.Console.Add($"Downloaded {savePath}");
                    }
                    catch (Exception ex)
                    {
                        downloadErrorResponse = new PackageTransferResponse
                        {
                            Exception = ex,
                            Result = PackageTransferResultTypes.FileDownloadFailed,
                            Message = $"Failed to download file {missingFile.Path}"
                        };
                    }
                });

                //foreach (PackageFile missingFile in filesToDownload)
                if (downloadErrorResponse != null) 
                    return downloadErrorResponse;
            }



            // write state to package
            _package.TransferState = PackageTransferStates.Downloaded;

            // todo : log more info about transfer time etc?

            return new PackageTransferResponse {
                Succeeded = true
            };
        }

        private bool VerifyAndCopyFile(string donorFilePath, string newFilePath, string donorSHA256, string packageId, bool force = false)
        {
            // Check donor file exists
            if (!File.Exists(donorFilePath))
            {
                _log.LogDebug($"Server common list error or doner file missing - expected file {donorFilePath} not found - downloading instead");
                return false;
            }

            if (File.Exists(newFilePath) && !force)
            {
                return true;
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
