namespace TetrifactClient
{
    /// <summary>
    /// Sliding states for build. A build always has one and only one of these.
    /// </summary>
    public enum BuildTransferStates
    {
        AvailableForDownload,       // default state, build exists remotely
        UserIgnored,                // user explicitly states they do not want to download build 
        AutoTagIgnored,             // ignored because of tags
        AutoQueueForDownload,       // build queued automatically
        UserQueuedForDownload,      // user explicitly queued build for download
        Downloading,                // build is currently being downloaded
        DownloadFailed,             // build transfer failed and willnot resume
        Downloaded,                 // build is available locally for starting
        DoNotDelete,                // build is downloaded and marked as keep forever by user
        Deleting,                   // build is queued for delete
        Deleted                     // build was deleted
    }
}
