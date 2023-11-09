namespace TetrifactClient
{
    /// <summary>
    /// Sliding states for build. A build always has one and only one of these.
    /// </summary>
    public enum BuildTransferStates
    {
        AvailableForDownload,       // default state, package exists remotely
        UserIgnored,                // user explicitly states they do not want to download package
        AutoTagIgnored,             // ignored because of tags
        AutoMarkedForDownload,       // package queued automatically
        UserMarkedForDownload,      // user explicitly queued package for download
        Downloading,                // package is currently being downloaded
        DownloadFailed,             // package transfer failed and willnot resume
        Downloaded,                 // package is available locally for starting
        DoNotDelete,                // package is downloaded and marked as keep forever by user
        AutoMarkedForDelete,        // package is marked is marked for delete by daemon
        UserMarkedForDelete,        // package is marked for delete by user
        Deleting,                   // package is queued for delete
        Deleted,                    // package was deleted
        DeleteFailed                // delete failed
    }
}
