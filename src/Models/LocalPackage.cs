using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;

namespace TetrifactClient
{
    /// <summary>
    /// Represents a local version of a package. 
    /// </summary>
    public partial class LocalPackage : ObservableObject
    {
        #region PROPERTIES

        /// <summary>
        /// Server package will be pulled from.
        /// </summary>
        [property: JsonProperty("TetrifactServerAddress")]
        [ObservableProperty]
        private string _tetrifactServerAddress;

        /// <summary>
        /// 
        /// </summary>
        [property: JsonProperty("Ignore")]
        [ObservableProperty]
        private bool _ignore;

        /// <summary>
        /// 
        /// </summary>
        [property: JsonProperty("TransferState")]
        [property: JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        [ObservableProperty]
        private PackageTransferStates _transferState;

        /// <summary>
        /// If transfer or integrity check failed, message describing failure.
        /// </summary>
        [property: JsonProperty("ErrorSummary")]
        [ObservableProperty]
        private string _errorSummary;

        /// <summary>
        /// Core of remote package, on tetrifact. Files are not serialized to this object, as this would create a performance bottleneck.
        /// </summary>
        [property: JsonProperty("Package")]
        [ObservableProperty]
        [JsonIgnore]
        public Package _package;

        /// <summary>
        /// Not observable, not persisted to JSON file.
        /// </summary>
        [property: JsonIgnore]
        public string DiskPath { get; set; }

        /// <summary>
        /// User-friendly description of what is going on with this package now. Not persisted to json.
        /// </summary>
        [property: JsonProperty("Status")]
        [ObservableProperty]
        [property: JsonIgnore]
        private string _status;

        #endregion

        public LocalPackage() 
        {
        }

        #region METHODS

        public void EnableAutoSave() 
        {
            PropertyChanged += this.OnPropertyChanged;
        }

        public void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) 
        {
            try
            {
                if (!string.IsNullOrEmpty( this.DiskPath))
                    File.WriteAllText(this.DiskPath, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            catch (Exception ex)
            {
                GlobalDataContext.Instance.Console.Add(ex.ToString());
            }
        }

        /// <summary>
        /// File path full package zip is written to. If this path exists, assume download has already be performed successfully.
        /// </summary>
        /// <returns></returns>
        public string GetFullDownloadFilePath() 
        {
           
            return string.Empty;
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
                || this.TransferState == PackageTransferStates.Downloading
                || this.TransferState == PackageTransferStates.AutoMarkedForDownload
                || this.TransferState == PackageTransferStates.UserMarkedForDownload;
        }

        public bool CanBeAutoCleanedUp() 
        {
            if (this.TransferState == PackageTransferStates.DoNotDelete)
                return false;

            return this.TransferState == PackageTransferStates.Downloaded;
        }

        public bool IsMarkedForDelete() 
        {
            return this.TransferState == PackageTransferStates.AutoMarkedForDelete
                || this.TransferState == PackageTransferStates.UserMarkedForDelete;
        }

        public bool IsQueuedForDownload() 
        {
            return this.TransferState == PackageTransferStates.UserMarkedForDownload
                || this.TransferState == PackageTransferStates.AutoMarkedForDownload;
        }

        #endregion
    }
}
