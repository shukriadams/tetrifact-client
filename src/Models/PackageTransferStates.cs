namespace TetrifactClient
{
    /// <summary>
    /// Sliding states for build. A build always has one and only one of these.
    /// </summary>
    public enum PackageTransferStates
    {
        AvailableForDownload,       // default state, package exists remotely
        AutoMarkedForDownload,       // package queued automatically
        UserMarkedForDownload,      // user explicitly queued package for download
        UserCancellingDownload,     // user has requested package download be cancelled. this state can be entered into only if pacakge is queued for downloading, or is downloading
        DownloadFailed,             // package transfer failed and willnot resume
        Downloaded,                 // package is available locally for starting
        Corrupt,                    // package is available locally but has failed verification
        DownloadCancelled,          // download was successfully cancelled
        DoNotDelete,                // package is downloaded and marked as keep forever by user
        AutoMarkedForDelete,        // package is marked is marked for delete by daemon
        UserMarkedForDelete,        // package is marked for delete by user
        Deleting,                   // package is queued for delete
        Deleted,                    // package was deleted
        DeleteFailed                // delete failed
    }
}
