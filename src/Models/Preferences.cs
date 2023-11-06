using Newtonsoft.Json;
using System;
using System.IO;

namespace TetrifactClient
{
    public partial class Preferences
    {
        #region PROPERTIES

        public bool AutomaticallyRunAtStartup { get; set; }

        /// <summary>
        /// Directory where full package zips are downloaded to.
        /// </summary>
        public string ProjectsRootDirectory { get; set; }

        /// <summary>
        /// Not sure this is cross-OS
        /// </summary>
        public bool EnableDesktopNotifications { get; set; }

        public bool DownloadWhilePackageRunning { get; set; }

        public string LogLevel { get; set; }

        public int ParallelDownloadThreads { get; set; }

        public int DownloadChunkSize { get; set; }

        #endregion

        #region CTORS

        public Preferences()
        {
            this.ProjectsRootDirectory = string.Empty; // Path.Combine(globalDataContext.DataFolder, "projects");
            this.ParallelDownloadThreads = 10; // todo : less hardcoding of this
            this.LogLevel = string.Empty;
            this.DownloadChunkSize = 10000000;
        }

        #endregion

        #region METHODS

        public void Save() 
        {
            try
            {
                string raw = JsonConvert.SerializeObject(this);
                string path = Path.Combine(PathHelper.GetInternalDirectory(), "settings.json");
                File.WriteAllText(path, raw);
            }
            catch (Exception ex)
            {
                Alert alert = new Alert();
                alert.SetContent("Critical error", $"Could not save settings.json to disk : {ex}");
                alert.ShowDialog(MainWindow.Instance);
            }
        }

        #endregion
    }
}
