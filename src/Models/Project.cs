using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace TetrifactClient
{
    public partial class Project : ObservableObject
    {
        #region FIELDS

        [property: JsonProperty("Name")]
        [ObservableProperty]
        private string _name;

        [property: JsonProperty("Id")]
        [ObservableProperty]
        private string _id;

        [property: JsonProperty("BuildServer")]
        [ObservableProperty]
        private string _buildServer;

        [property: JsonProperty("ApplicationExecutableName")]
        [ObservableProperty]
        private string _applicationExecutableName;

        /// <summary>
        /// Name of application process in windows. Used to track running instance of application and terminate if necessary.
        /// </summary>
        [property: JsonProperty("ApplicationProcessName")]
        [ObservableProperty]
        private string _applicationProcessName;

        [property: JsonProperty("DiffDownloadThreshold")]
        [ObservableProperty]
        private int _diffDownloadThreshold;

        /// <summary>
        /// Does not persist
        /// </summary>
        [ObservableProperty]
        private string _serverErrorDescription;

        [property: JsonProperty("MaxDownloadFailedAttempts")]
        [ObservableProperty]
        private int? _maxDownloadFailedAttempts;

        /// <summary>
        /// Comma-separted tags remote packages must have to be eligable for download.
        /// </summary>
        [property: JsonProperty("RequiredTags")]
        [ObservableProperty]
        private string _requiredTags;

        /// <summary>
        /// Comma-separted tags remote packages will be ignored on.
        /// </summary>
        [property: JsonProperty("IgnoreTags")]
        [ObservableProperty]
        private string _ignoreTags;

        /// <summary>
        /// Access key for tetrifact server instances that are access protected.
        /// </summary>
        [property: JsonProperty("AccessKey")]
        [ObservableProperty]
        private string _accessKey;

        /// <summary>
        /// Loaded on-the-fly by daemons
        /// </summary>
        [ObservableProperty]
        private IList<Package> _packages;

        /// <summary>
        /// Packages available remotely. Details need to be retrieved.
        /// Loaded on-the-fly by daemons
        /// </summary>
        [ObservableProperty]
        private IList<string> _availablePackages;

        [property: JsonProperty("ServerState")]
        [ObservableProperty]
        private SourceServerStates _serverState;

        #endregion

        #region CTORS

        public Project() 
        {
            this.Id = Guid.NewGuid().ToString();
            this.Packages = new List<Package>();
            this.AvailablePackages = new List<string>();
        }

        #endregion

        #region METHODS

        public void ListPackages() 
        {
            string localProjectPackagesDirectory = Path.Combine(GlobalDataContext.Instance.GetProjectsDirectoryPath(), this.Id, "packages");
            if (!Directory.Exists(localProjectPackagesDirectory))
                return;

            IEnumerable<string> packages = Directory.
                GetDirectories(localProjectPackagesDirectory).
                Select(p => Path.GetFileName(p));

            List<Package> newPackages = new List<Package>();
            foreach (string package in packages)
            {
                if (this.Packages.Any(p => p.Id == package))
                    continue;

                string packageRawJson = string.Empty;
                string basefilePath = Path.Combine(localProjectPackagesDirectory, package, "base.json");
                if (!File.Exists(basefilePath))
                    continue;

                try
                {
                    packageRawJson = File.ReadAllText(basefilePath);
                }
                catch (Exception ex)
                {
                    // todo : handle error
                    throw;
                }

                try
                {
                    Package packageObject = JsonConvert.DeserializeObject<Package>(packageRawJson);
                    if (packageObject == null)
                        throw new Exception($"Failed to load JSON for package {package}");

                    this.Packages.Add(packageObject);

                }
                catch (Exception ex)
                {
                    // todo : handle
                    throw;
                }
            }

            this.Packages = this.Packages.OrderByDescending(p => p.CreatedUtc).ToList(); ;
        }

        #endregion
    }
}
