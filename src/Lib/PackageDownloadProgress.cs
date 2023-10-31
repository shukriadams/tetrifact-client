namespace TetrifactClient
{
    /// <summary>
    /// Information about package download progress. For UX / UI. 
    /// </summary>
    public class PackageDownloadProgress
    {
        /// <summary>
        /// Id of package being downloaded
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        /// Id of project package is being downloaded for.
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        /// If downloading full achive, current size of that download. This is determined by periodically reading the size of the file on disk.
        /// </summary>
        public long ArchiveDownloadedSoFar { get; set; }

        /// <summary>
        /// Total nr of files in build. For full archive download, this is known when archive unpackaing begins. For partial downloading, this known
        /// at process start, as we already have a master diff list to download with.
        /// </summary>
        public int TotalFiles { get; set; }

        /// <summary>
        /// Files package process, as a fraction of this.TotalFiles.
        /// </summary>
        public int FilesSoFar { get; set; }

        /// <summary>
        /// True if fetching full zip. If not, will fetch individual files and copy others from local builds.
        /// </summary>
        public bool FullDownload { get; set; }

        /// <summary>
        /// Desription about download, such as "Waiting for server", "Downloading zip", "Copying from local", "Downloading files", "Unpacking zip". 
        /// This is for user info only.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// False if download failed, details should be appended to message
        /// </summary>
        public bool Succeeded { get; set; }
    }
}
