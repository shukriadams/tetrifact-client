using System;
using System.IO.Compression;
using System.IO;
using System.Linq;

namespace TetrifactClient
{
    public class PackageUnzip : ICancel
    {
        #region FIELDS

        private string _zipFilePath;

        private GlobalDataContext _context;

        private LocalPackage _package;

        private Project _project;

        private string _unzipPathTempPath;

        private string _unzipFinalPath;

        #endregion

        #region PROPERTIES

        public long Total { get; private set; }

        public long Progress { get; private set; }

        public long ProgressPercent { get; private set; }
        
        public IsTrueLookup CancelCheck { get; set; }

        #endregion

        #region CTORS

        /// <summary>
        /// Unpacks a zip file to the given path
        /// </summary>
        /// <param name="archivePath"></param>
        /// <param name="targetPath"></param>
        public PackageUnzip(GlobalDataContext context, Project project, LocalPackage package, string zipFilePath)
        {
            _zipFilePath = zipFilePath;
            _unzipPathTempPath = PathHelper.GetPackageDirectoryTempPath(context, project, package);
            _unzipFinalPath = PathHelper.GetPackageContentDirectoryPath(context, project, package);
            _package = package;
            _project = project;
            _context = context;
        }

        #endregion

        #region METHODS

        private void DoUnzip()
        {
            _package.DownloadProgress.Message = "Unpacking";

            using (FileStream file = new FileStream(_zipFilePath, FileMode.Open))
            using (ZipArchive archive = new ZipArchive(file))
            {
                this.Total = archive.Entries.Where(r => r != null).Select(r => r.Length).Count();

                // store size of build archive, this will be used to estimate download progress on next build
                _package.DownloadProgress.Total = archive.Entries.Count;

                this.Progress = 0;

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    // break out of look if cancel set from parent process
                    if (this.CancelCheck != null && this.CancelCheck())
                        continue;

                    if (entry == null)
                        continue;

                    _package.DownloadProgress.Progress ++;
                    _package.DownloadProgress.Message = $"Unpacking {MathHelper.Percent(_package.DownloadProgress.Progress, this.Total)}%";

                    using (Stream unzippedEntryStream = entry.Open())
                    {
                        string targetFile = Path.Combine(_unzipPathTempPath, entry.FullName);
                        string targetDirectory = Path.GetDirectoryName(targetFile);

                        // if .Name is empty ignore because it's a directory
                        if (string.IsNullOrEmpty(entry.Name))
                            continue;

                        FileSystemHelper.CreateDirectory(targetDirectory);

                        if (targetFile.Length > Constants.MAX_PATH_LENGTH)
                            throw new Exception($"{targetFile} exceeds max safe path length");

                        using (FileStream filestream = new FileStream(targetFile, FileMode.CreateNew))
                        {
                            Stream zipStream = entry.Open();

                            byte[] buffer = new byte[1024];
                            int read;

                            try
                            {
                                while ((read = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    filestream.Write(buffer, 0, read);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Unpack()
        {
            // this check is probably necessary, as calling could should already exit if this dir exists
            if (Directory.Exists(_unzipFinalPath))
                Directory.Delete(_unzipFinalPath, true);

            // if temp unzip directory exists, assume previous unzip attempt failed and is left hanging on disk
            if (Directory.Exists(_unzipPathTempPath))
                Directory.Delete(_unzipPathTempPath, true);

            this.DoUnzip();

            // clean up zip file
            File.Delete(_zipFilePath);

            // flip directory to "done" state
            Directory.Move(_unzipPathTempPath, _unzipFinalPath);
        }

        #endregion
    }
}
