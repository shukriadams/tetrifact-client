using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
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

        [property: JsonProperty("TetrifactServerAddress")]
        [ObservableProperty]
        private string _tetrifactServerAddress;

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
        [property: Newtonsoft.Json.JsonIgnore] // need this defined twice for autogen and local 
        [JsonIgnore]                            // need this defined twice for autogen and local 
        public ObservableCollection<LocalPackage> _packages;

        /// <summary>
        /// Ids of all packages available remotely. This list is unfiltered. Details need to be retrieved.
        /// Loaded on-the-fly by daemons
        /// </summary>
        [ObservableProperty]
        [property: Newtonsoft.Json.JsonIgnore]
        private IList<string> _availablePackageIds;

        [property: JsonProperty("ServerState")]
        [property: JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        [ObservableProperty]
        private SourceServerStates _serverState;

        #endregion

        #region CTORS

        public Project() 
        {
            this.Id = Guid.NewGuid().ToString();
            this.Packages = new ObservableCollection<LocalPackage>();
            this.AvailablePackageIds = new List<string>();
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Populates packages collection from disk
        /// </summary>
        public void PopulatePackageList()
        {
                string localProjectPackagesDirectory = Path.Combine(GlobalDataContext.Instance.GetProjectsDirectoryPath(), this.Id, "packages");
                if (!Directory.Exists(localProjectPackagesDirectory))
                    return;

                IEnumerable<string> packageIds = Directory.
                    GetDirectories(localProjectPackagesDirectory).
                    Select(p => Path.GetFileName(p));

                List<LocalPackage> newPackages = new List<LocalPackage>();
                foreach (string packageId in packageIds)
                {
                    if (this.Packages.Any(p => p.Package.Id == packageId))
                        continue;

                    string packageRawJson = string.Empty;
                    string packageCorePath = Path.Combine(localProjectPackagesDirectory, packageId, "remote.json");
                    if (!File.Exists(packageCorePath))
                        continue;

                    JsonFileLoadResponse<LocalPackage> baseLoadReponse = JsonHelper.LoadJSONFile<LocalPackage>(packageCorePath, true, true);

                    // Todo : handle error bettter
                    if (baseLoadReponse.ErrorType != JsonFileLoadResponseErrorTypes.None)
                        throw new Exception($"failed to load {packageCorePath}, {baseLoadReponse.ErrorType} {baseLoadReponse.Exception}");

                    baseLoadReponse.Payload.DiskPath = packageCorePath;
                    newPackages.Add(baseLoadReponse.Payload);
                }

                // sort and apply filters
                string requiredTags = string.IsNullOrEmpty(this.RequiredTags) ? string.Empty : this.RequiredTags;
                string ignoreTags = string.IsNullOrEmpty(this.IgnoreTags) ? string.Empty : this.RequiredTags;
                string[] requiredTagsArray = requiredTags.Split(",", StringSplitOptions.RemoveEmptyEntries);
                string[] ignoreTagsArray = ignoreTags.Split(",", StringSplitOptions.RemoveEmptyEntries);

                IEnumerable<LocalPackage> tempPackages = newPackages.OrderByDescending(p => p.Package.CreatedUtc);
                if (requiredTags.Any())
                    tempPackages = from package in tempPackages
                                   where !package.Package.Tags.Except(requiredTagsArray).Any()
                                   select package;

                if (ignoreTagsArray.Any())
                    tempPackages = from package in tempPackages
                                   where package.Package.Tags.Except(ignoreTagsArray).Any()
                                   select package;

               if (!tempPackages.Any())
                    return;
                
                foreach (var package in tempPackages) 
                {
                    this.Packages.Add(package);
                    package.EnableAutoSave();
                }

                //tempPackages = tempPackages.OrderByDescending(p => p.Package.CreatedUtc);
                //project.Packages = new ObservableCollection<LocalPackage> (tempPackages);
                System.Diagnostics.Debug.WriteLine($"PopulatePackageList:{DateTime.Now.Second}:{this.Packages.Count}");

        }

        #endregion
    }
}
