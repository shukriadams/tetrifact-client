using CommunityToolkit.Mvvm.ComponentModel;

namespace TetrifactClient
{
    /// <summary>
    /// Information about package download/unzipping/verifying progress. For UX / UI. 
    /// </summary>
    public partial class PackageTransferProgress : ObservableObject
    {
        /// <summary>
        /// bytes or files already processed.
        /// </summary>
        [ObservableProperty]
        private long _progress;

        /// <summary>
        /// Total nr of bytes or files needing to be processed.
        /// </summary>
        [ObservableProperty]
        private long _total;

        /// <summary>
        /// True if fetching full zip. If not, will fetch individual files and copy others from local builds.
        /// </summary>
        [ObservableProperty]
        private bool _isFullDownload;

        /// <summary>
        /// Desription about download, such as "Waiting for server", "Downloading zip", "Copying from local", "Downloading files", "Unpacking zip". 
        /// This is for user info only.
        /// </summary>
        [ObservableProperty]
        private string _message;

        public PackageTransferProgress() 
        {
            this.Message = string.Empty;
        }
    }
}
