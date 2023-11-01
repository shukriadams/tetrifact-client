using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Newtonsoft.Json;
using TetrifactClient.Models;

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

        /// <summary>
        /// Number of packages to keep locally synced.
        /// </summary>
        [property: JsonProperty("PackageSyncCount")]
        [ObservableProperty]
        private int _packageSyncCount;

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
        /// packages for this project should be automatically downloaded
        /// </summary>
        [property: JsonProperty("Autodownload")]
        [ObservableProperty]
        private bool _autoDownload;

        [property: JsonProperty("PurgeOldPackages")]
        [ObservableProperty]
        public bool _purgeOldPackages;

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
        /// Loaded on-the-fly by daemons, does not persist. Exposed as .Packages
        /// </summary>
        [ObservableProperty]
        private IList<Package> _packages;

        /// <summary>
        /// Ids of all packages available remotely. This list is unfiltered. Details need to be retrieved.
        /// Loaded on-the-fly by daemons
        /// </summary>
        [ObservableProperty]
        private IList<string> _availablePackageIds;

        [property: JsonProperty("ServerState")]
        [ObservableProperty]
        private SourceServerStates _serverState;

        #endregion

        #region CTORS

        public Project() 
        {
            this.Id = Guid.NewGuid().ToString();
            this.Packages = new List<Package>();
            this.AvailablePackageIds = new List<string>();
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Populates packages collection with available projects
        /// </summary>
        public void PopulateAvailableProjectsList()
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

                JsonFileLoadResponse<Package> baseLoadReponse = JsonHelper.LoadJSONFile<Package>(basefilePath, true, true);
                // Todo : handle error bettter
                if (baseLoadReponse.ErrorType != JsonFileLoadResponseErrorTypes.None)
                    throw new Exception($"failed to load {basefilePath}, {baseLoadReponse.ErrorType} {baseLoadReponse.Exception}");

                newPackages.Add(baseLoadReponse.Payload);

                // look for local package state
                string localPackageStatePath = Path.Combine(localProjectPackagesDirectory, package, "local.json");
                if (!File.Exists(localPackageStatePath))
                    continue;

                JsonFileLoadResponse<LocalPackage> localPackageLoadResponse = JsonHelper.LoadJSONFile<LocalPackage>(localPackageStatePath, true, true);
                // todo : handle this error with less suck
                if (localPackageLoadResponse.ErrorType != JsonFileLoadResponseErrorTypes.None)
                    throw new Exception($"failed to load {localPackageStatePath}, {localPackageLoadResponse.ErrorType} {localPackageLoadResponse.Exception}");

                baseLoadReponse.Payload.LocalPackage = localPackageLoadResponse.Payload;
            }

            // sort and apply filters
            string requiredTags = string.IsNullOrEmpty(this.RequiredTags) ? string.Empty : this.RequiredTags;
            string ignoreTags  = string.IsNullOrEmpty(this.IgnoreTags) ? string.Empty : this.RequiredTags;
            string[] requiredTagsArray = requiredTags.Split(",", StringSplitOptions.RemoveEmptyEntries);
            string[] ignoreTagsArray = ignoreTags.Split(",", StringSplitOptions.RemoveEmptyEntries);

            IEnumerable<Package> tempPackages = newPackages.OrderByDescending(p => p.CreatedUtc);
            if (requiredTags.Any())
                tempPackages = from package in tempPackages 
                    where !package.Tags.Except(requiredTagsArray).Any()
                    select package;

            if (ignoreTagsArray.Any())
                tempPackages = from package in tempPackages
                               where package.Tags.Except(ignoreTagsArray).Any()
                               select package;

            if (!tempPackages.Any())
                return;

            foreach (var package in tempPackages)
                this.Packages.Add(package);

            this.Packages = tempPackages.ToList();
            this.Packages = this.Packages
                .OrderByDescending(p => p.CreatedUtc)
                .ToList();
        }

        #endregion
    }
}
;