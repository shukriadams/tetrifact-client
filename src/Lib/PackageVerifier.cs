using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace TetrifactClient
{
    /// <summary>
    /// Process for running a checksum on all files in a package. Checksum output must match the checksum of the source package on tetrifact.
    /// </summary>
    public class PackageVerifier
    {
        #region FIELDS

        private Project _project;
        
        private LocalPackage _localPackage;

        private string _packageId;

        private string _packagePath;

        private string _serverAddress;

        private int _currentFile;

        private int _totalFiles;

        private int _threads;

        public event OnProgress OnChunkDownloaded;

        public event OnProgress OnEnd;

        private ILog _log;

        #endregion

        #region CTORS

        public PackageVerifier(Project project, LocalPackage localPackage, string packagePath, string serverAddress, string packageId, int threads)
        {
            _project = project;
            _localPackage = localPackage;
            _serverAddress = serverAddress;
            _packageId = packageId;
            _packagePath = packagePath;
            _threads = threads;
            _log = App.UnityContainer.Resolve<ILog>();
        }

        #endregion

        #region METHODS
        
        public void Start()
        {
            Task.Run(async () => this.Work());
        }

        /// <summary>
        /// Calculates the checksum of an entire package, using Tetrifact's checksum calculation.
        /// 1) Sort all files alphaberically by path name relative to package root directory
        /// 2) generate an md5 checksum of the file on-disk
        /// 3) Concat the path + the file checksum to a total string
        /// 4) generate a checksum of the total string
        /// </summary>
        private async Task Work()
        {
            IList<string> errors = new List<string>();

            // download manifest
            Package package = JsonHelper.DownloadManifest(_serverAddress, _packageId);

            // file must be alphabetically sorted for total package checksum
            Array.Sort(package.Files.ToArray(), (x, y) => String.Compare(x.Path, y.Path));

            Dictionary<int, OrderedHash> orderedHash = new Dictionary<int, OrderedHash>();

            int i = 0;
            foreach (PackageFile file in package.Files)
            {
                orderedHash.Add(i, new OrderedHash { File = file });
                i++;
            }

            StringBuilder totalChecksum = new StringBuilder();
            _totalFiles = package.Files.Count();

            Parallel.ForEach(orderedHash.Values, new ParallelOptions() { MaxDegreeOfParallelism = _threads }, item =>
            {
                try
                {
                    string expectedPath = Path.Combine(_packagePath, item.File.Path);
                    if (!File.Exists(expectedPath))
                    {
                        lock (errors)
                            errors.Add($"Expected file {expectedPath} not present");

                        return;
                    }

                    string checksum = HashLib.FromFile(expectedPath);
                    if (checksum != item.File.Hash)
                        lock (errors)
                            errors.Add($"Checksum for file {expectedPath} failed - expected {item.File.Hash}, got {checksum}");

                    item.LocalHash = HashLib.FromString(item.File.Path) + item.File.Hash;
                }
                finally
                {
                    this.HandleChunkDownloadEvent();
                }
            });

            foreach (KeyValuePair<int, OrderedHash> h in orderedHash)
                totalChecksum.Append(h.Value.LocalHash);

            string packageHashOnDisk = HashLib.FromString(totalChecksum.ToString());
            if (packageHashOnDisk != package.Hash)
                errors.Add($"Package checksum on disk differs from remote - expected {package.Hash}, got {packageHashOnDisk}");
                
            if (errors.Any())
            {
                _log.LogError($"Local Build {package.Id} failed checksum");
                _log.LogError(String.Join(",", errors));
            }
        }

        private void HandleChunkDownloadEvent()
        {
            decimal p = (decimal)_currentFile / (decimal)_totalFiles;
            int percent = (int)Math.Round((decimal)(p * 100), 0);

            PackageTransferProgress buildStatus = PackageTransferProgressStore.Get(_project, _localPackage);
            buildStatus.Message = $"Verify {percent}%";

            _currentFile++;
        }

        #endregion
    }
}
