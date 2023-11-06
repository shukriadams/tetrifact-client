using System.Collections.Generic;
using System.Linq;

namespace TetrifactClient
{
    /// <summary>
    /// Represents a local version of a package. 
    /// </summary>
    public class LocalPackage
    {
        #region PROPERTIES

        /// <summary>
        /// Server package will be pulled from.
        /// </summary>
        public string TetrifactServerAddress { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BuildTransferStates TransferState { get; set; }

        /// <summary>
        /// If transfer or integrity check failed, message describing failure.
        /// </summary>
        public string ErrorSummary { get; set; }

        /// <summary>
        /// Core of remote package, on tetrifact. Files are not serialized to this object, as this would create a performance bottleneck.
        /// </summary>
        public Package Package { get; set; }

        #endregion

        #region METHODS

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
            return this.TransferState == BuildTransferStates.AvailableForDownload;
        }

        public bool IsExecutable()
        {
            return this.TransferState == BuildTransferStates.DoNotDelete 
                || this.TransferState == BuildTransferStates.Downloaded;
        }

        public bool IsDownloadedorQueuedForDownload()
        {
            return this.TransferState == BuildTransferStates.Downloaded
                || this.TransferState == BuildTransferStates.Downloading
                || this.TransferState == BuildTransferStates.AutoQueueForDownload
                || this.TransferState == BuildTransferStates.UserQueuedForDownload;
        }

        public bool CanBeAutoCleanedUp() 
        {
            if (this.TransferState == BuildTransferStates.DoNotDelete)
                return false;

            return this.TransferState == BuildTransferStates.Downloaded;
        }

        public bool IsMarkedForDelete() 
        {
            return this.TransferState == BuildTransferStates.AutoMarkedForDelete
                || this.TransferState == BuildTransferStates.UserMarkedForDelete;
        }

        public bool IsQueuedForDownload() 
        {
            return this.TransferState == BuildTransferStates.UserQueuedForDownload
                || this.TransferState == BuildTransferStates.AutoQueueForDownload;
        }

        #endregion
    }
}
