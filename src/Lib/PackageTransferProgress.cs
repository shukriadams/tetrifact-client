namespace TetrifactClient
{
    /// <summary>
    /// Information about package download/unzipping/verifying progress. For UX / UI. 
    /// </summary>
    public class PackageTransferProgress
    {
        /// <summary>
        /// Id of package being downloaded
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Project Project { get; set; }

        /// <summary>
        /// bytes or files already processed.
        /// </summary>
        public long Progress { get; set; }

        /// <summary>
        /// Total nr of bytes or files needing to be processed.
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// True if fetching full zip. If not, will fetch individual files and copy others from local builds.
        /// </summary>
        public bool IsFullDownload { get; set; }

        /// <summary>
        /// Desription about download, such as "Waiting for server", "Downloading zip", "Copying from local", "Downloading files", "Unpacking zip". 
        /// This is for user info only.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
