using System.IO;

namespace TetrifactClient
{
    public partial class Preferences
    {
        #region PROPERTIES

        public bool AutomaticallyRunAtStartup { get; set; }

        public string DownloadsFolder { get; set; }

        public string UnpackFolder { get; set; }

        /// <summary>
        /// Not sure this is cross-OS
        /// </summary>
        public bool EnableDesktopNotifications { get; set; }

        public bool DownloadWhilePackageRunning { get; set; }

        public string LogLevel { get; set; }

        #endregion

        #region CTORS

        public Preferences(GlobalDataContext globalDataContext)
        {
            this.DownloadsFolder = Path.Combine(globalDataContext.DataFolder, "packages");
            this.UnpackFolder = this.DownloadsFolder;
            this.PartialDownloads = true;
            this.LogLevel = string.Empty;
        }

        #endregion
    }
}
