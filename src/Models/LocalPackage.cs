using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;

namespace TetrifactClient
{
    /// <summary>
    /// Represents a local version of a package. 
    /// </summary>
    public class LocalPackage : Observable
    {
        #region FIELDS

        private string _tetrifactServerAddress;
        private bool _ignore;
        private bool _isDownloading;
        private bool _isQueuedForVerify;
        private bool _keep;
        private PackageTransferStates _transferState;
        private string _errorSummary;
        private Package _package;
        private string _status;
        private PackageTransferProgress _downloadProgress;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Server package will be pulled from.
        /// </summary>
        public string TetrifactServerAddress
        {
            get => _tetrifactServerAddress;
            set
            {
                _tetrifactServerAddress = value;
                OnPropertyChanged(nameof(TetrifactServerAddress));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Ignore
        {
            get => _ignore;
            set
            {
                _ignore = value;
                OnPropertyChanged(nameof(Ignore));
            }
        }

        public bool IsQueuedForVerify 
        {
            get => _isQueuedForVerify;
            set
            {
                _isQueuedForVerify = value;
                OnPropertyChanged(nameof(IsQueuedForVerify));
            }
        }

        /// <summary>
        /// Marked as true when downloading starts, does
        /// </summary>
        [JsonIgnore]
        public bool IsDownloading
        {
            get => _isDownloading;
            set
            {
                _isDownloading = value;
                OnPropertyChanged(nameof(IsDownloading));
            }
        }

        public bool Keep
        {
            get => _keep;
            set
            {
                _keep = value;
                OnPropertyChanged(nameof(Keep));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [property: JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public PackageTransferStates TransferState
        {
            get => _transferState;
            set
            {
                _transferState = value;
                OnPropertyChanged(nameof(TransferState));
            }
        }

        public string ErrorSummary
        {
            get => _errorSummary;
            set
            {
                _errorSummary = value;
                OnPropertyChanged(nameof(ErrorSummary));
            }
        }

        /// <summary>
        /// Core of remote package, on tetrifact. Files are not serialized to this object, as this would create a performance bottleneck.
        /// </summary>
        public Package Package
        {
            get => _package;
            set
            {
                _package = value;
                OnPropertyChanged(nameof(Package));
            }
        }

        /// <summary>
        /// Not observable, not persisted to JSON file.
        /// </summary>
        [JsonIgnore]
        public string DiskPath { get; set; }

        /// <summary>
        /// User-friendly description of what is going on with this package now. Not persisted to json.
        /// 
        /// PHASE OUT
        /// </summary>
        [Obsolete]
        [JsonIgnore]
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        /// <summary>
        /// do not persist
        /// </summary>
        [JsonIgnore]
        public PackageTransferProgress DownloadProgress 
        {
            get => _downloadProgress;
            set
            {
                _downloadProgress = value;
                OnPropertyChanged(nameof(DownloadProgress));
            }
        }

        #endregion

        #region CTORS
            
        public LocalPackage() 
        {
            this.DownloadProgress = new PackageTransferProgress();
        }

        #endregion

        #region METHODS

        private bool saveset = false;

        public void EnableAutoSave() 
        {
            if (saveset)
                return;

            PropertyChanged += this.OnPropertyChanged;
            saveset = true;
        }

        public void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) 
        {
            if (!string.IsNullOrEmpty( this.DiskPath))
                File.WriteAllText(this.DiskPath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        /// <summary>
        /// File path full package zip is written to. If this path exists, assume download has already be performed successfully.
        /// </summary>
        /// <returns></returns>
        public string GetFullDownloadFilePath() 
        {
            return string.Empty;
        }

        public bool IsDownloadable() 
        {
            return this.TransferState == PackageTransferStates.AvailableForDownload
                || this.TransferState == PackageTransferStates.Deleted
                || this.TransferState == PackageTransferStates.DownloadFailed;
        }

        /// <summary>
        /// Directory path final package is placed in. This is the location that exe will be run from, and also which will be deleted
        /// when package is cleaned up.
        /// </summary>
        /// <returns></returns>
        public string GetExecuteDirectoryPath()
        {
            return string.Empty;
        }

        public bool IsEligibleForAutoDownload()
        {
            return this.TransferState == PackageTransferStates.AvailableForDownload;
        }

        public bool IsExecutable()
        {
            return this.TransferState == PackageTransferStates.DoNotDelete 
                || this.TransferState == PackageTransferStates.Downloaded;
        }

        public bool IsDownloadedorQueuedForDownload()
        {
            return this.TransferState == PackageTransferStates.Downloaded
                || this.TransferState == PackageTransferStates.Corrupt
                || this.TransferState == PackageTransferStates.AutoMarkedForDownload
                || this.TransferState == PackageTransferStates.UserMarkedForDownload;
        }

        public bool CanBeAutoCleanedUp() 
        {
            if (this.TransferState == PackageTransferStates.DoNotDelete)
                return false;

            return this.TransferState == PackageTransferStates.Downloaded 
                || this.TransferState == PackageTransferStates.Corrupt;
        }

        public void CancelQueueState() 
        {
            if (!this.IsQueuedForDownloadorDownloading())
                return;

            this.TransferState = PackageTransferStates.UserCancellingDownload;
        }

        public bool IsMarkedForDeleteOrBeingDeleting() 
        {
            return this.TransferState == PackageTransferStates.AutoMarkedForDelete
                || this.TransferState == PackageTransferStates.UserMarkedForDelete
                || this.TransferState == PackageTransferStates.Deleting;
        }

        public bool IsQueuedForDownloadorDownloading()
        {
            return this.TransferState == PackageTransferStates.UserMarkedForDownload
                || this.TransferState == PackageTransferStates.AutoMarkedForDownload
                || this.IsDownloading;
        }

        public bool IsQueuedForDownload() 
        {
            return this.TransferState == PackageTransferStates.UserMarkedForDownload
                || this.TransferState == PackageTransferStates.AutoMarkedForDownload
                && !this.IsDownloading;
        }

        public bool IsDownloaded() 
        {
            return this.TransferState == PackageTransferStates.Downloaded
                || this.TransferState == PackageTransferStates.Corrupt;
        }

        public bool IsVerifiable() 
        {
            return this.IsDownloaded() && !this.IsQueuedForVerify;
        }

        #endregion
    }
}
