﻿using ReactiveUI;
using System.Linq;

namespace TetrifactClient.Models
{
    /// <summary>
    /// Represents a local version of a package. 
    /// </summary>
    public class LocalPackage
    {
        #region PROPERTIES

        /// <summary>
        /// Package id this object is connected to
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Ignore { get; set; }

        public BuildTransferStates TransferState { get; set; }

        /// <summary>
        /// If transfer or integrity check failed, message describing failure.
        /// </summary>
        public string ErrorSummary { get; set; }

        #endregion

        #region METHODS

        public bool IsEligibleForAutoDownload()
        {
            return this.TransferState == BuildTransferStates.AvailableForDownload;
        }

        public bool IsPlayable()
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

        public void Download(Project project) 
        {
            // find out if package download should be partial or full. Full is needed if no other package is available locally,
            // or if diff between this package and previous one is over a % of total files in build.
            //bool packageAlreadyDownlaoded = project.
        }

        #endregion
    }
}
