using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using Newtonsoft.Json;

namespace TetrifactClient
{
    public partial class Project : Observable
    {
        #region FIELDS

        private string _stat = string.Empty;
        private DateTime _statDate = DateTime.Now;
        private string _id;
        private string _name;
        private string _description;
        private string _tetrifactServerAddress;
        private int _packageSyncCount;
        private string _applicationExecutableName;
        private string _applicationProcessName;
        private int _diffDownloadThreshold;
        private string _serverErrorDescription;
        private int? _maxDownloadFailedAttempts;
        private bool _autoDownload;
        private bool _purgeOldPackages;
        private IEnumerable<string> _requiredTags;
        private IEnumerable<string> _ignoreTags;
        private string _accessKey;
        private ObservableCollection<LocalPackage> _packages;
        private IList<string> _availablePackageIds;
        private SourceServerStates _serverState;
        private string _currentStatus;
        private string _currentStatusDate;
        private IList<string> _commonTags;
        private readonly Dictionary<string, int> _rawTags = new Dictionary<string, int>();
        private IEnumerable<LocalPackage> _rawPackages = new List<LocalPackage>();
        private IList<string> _removeQueue = new List<string>();
        private IList<LocalPackage> _addQueue = new List<LocalPackage>();
        private ConnectionQuality _connectionQuality;
        private double _connectionSpeed;
        #endregion

        #region PROPERTIES

        /// <summary>
        /// Unique id of project. Generated from GUID. All data for project is partitition on disk with this id.
        /// </summary>
        public string Id
        {
            get => _id;
            set 
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            } 
        }

        /// <summary>
        /// Public name of project, not unique. Used for display purposes.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Optional description text of project. For user convenience.
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        /// <summary>
        /// Url of server to fetch packages from
        /// </summary>
        public string TetrifactServerAddress
        {
            get => _tetrifactServerAddress;
            set
            {
                _tetrifactServerAddress = value;
                OnPropertyChanged(nameof(TetrifactServerAddress));
            }
        }

        public int PackageSyncCount
        {
            get => _packageSyncCount;
            set
            {
                _packageSyncCount = value;
                OnPropertyChanged(nameof(PackageSyncCount));
            }
        }

        public string ApplicationExecutableName
        {
            get => _applicationExecutableName;
            set
            {
                _applicationExecutableName = value;
                OnPropertyChanged(nameof(ApplicationExecutableName));
            }
        }

        public string ApplicationProcessName
        {
            get => _applicationProcessName;
            set
            {
                _applicationProcessName = value;
                OnPropertyChanged(nameof(ApplicationProcessName));
            }
        }

        public int DiffDownloadThreshold
        {
            get => _diffDownloadThreshold;
            set
            {
                _diffDownloadThreshold = value;
                OnPropertyChanged(nameof(DiffDownloadThreshold));
            }
        }

        /// <summary>
        /// Persistent error in package operation. Leave blank if functional. 
        /// Does not persist to JSON.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string ServerErrorDescription
        {
            get => _serverErrorDescription;
            set
            {
                _serverErrorDescription = value;
                OnPropertyChanged(nameof(ServerErrorDescription));
            }
        }

        public int? MaxDownloadFailedAttempts
        {
            get => _maxDownloadFailedAttempts;
            set
            {
                _maxDownloadFailedAttempts = value;
                OnPropertyChanged(nameof(MaxDownloadFailedAttempts));
            }
        }

        /// <summary>
        /// packages for this project should be automatically downloaded
        /// </summary>
        public bool AutoDownload
        {
            get => _autoDownload;
            set
            {
                _autoDownload = value;
                OnPropertyChanged(nameof(AutoDownload));
            }
        }

        public bool PurgeOldPackages
        {
            get => _purgeOldPackages;
            set
            {
                _purgeOldPackages = value;
                OnPropertyChanged(nameof(PurgeOldPackages));
            }
        }

        /// <summary>
        /// Comma-separted tags remote packages must have to be eligable for download.
        /// </summary>
        public IEnumerable<string> RequiredTags
        {
            get => _requiredTags;
            set
            {
                _requiredTags = value;
                OnPropertyChanged(nameof(RequiredTags));
            }
        }

        /// <summary>
        /// Comma-separted tags remote packages will be ignored on. 
        /// </summary>
        public IEnumerable<string> IgnoreTags
        {
            get => _ignoreTags;
            set
            {
                _ignoreTags = value;
                OnPropertyChanged(nameof(IgnoreTags));
            }
        }

        /// <summary>
        /// Access key for tetrifact server instances that are access protected.
        /// </summary>
        public string AccessKey
        {
            get => _accessKey;
            set
            {
                _accessKey = value;
                OnPropertyChanged(nameof(AccessKey));
            }
        }

        /// <summary>
        /// Loaded on-the-fly by daemons, does not persist. Exposed as .Packages
        /// </summary>
        [JsonIgnore]                           
        public ObservableCollection<LocalPackage> Packages
        {
            get{
                lock(_packages)
                    return _packages;
            }
            private set
            {
                _packages = value;
                OnPropertyChanged(nameof(Packages));
            }
        }

        /// <summary>
        /// Ids of all packages available remotely. This list is unfiltered. Details need to be retrieved.
        /// Loaded on-the-fly by daemons
        /// </summary>
        [JsonIgnore]
        public IList<string> AvailablePackageIds
        {
            get => _availablePackageIds;
            set
            {
                _availablePackageIds = value;
                OnPropertyChanged(nameof(Packages));
            }
        }

        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public SourceServerStates ServerState
        {
            get => _serverState;
            set
            {
                _serverState = value;
                OnPropertyChanged(nameof(ServerState));
            }
        }

        /// <summary>
        /// Daemon processing this package will write status updates to this field. Use this to keep user informed about what is going on. Update
        /// often. This is not a log, but a way to make app feel responsive at all times.
        /// </summary>
        [JsonIgnore] 
        public string CurrentStatus
        {
            get => _currentStatus;
            set
            {
                _currentStatus = value;
                OnPropertyChanged(nameof(CurrentStatus));
            }
        }

        /// <summary>
        /// time of last status update.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string CurrentStatusDate
        {
            get => _currentStatusDate;
            set
            {
                _currentStatusDate = value;
                OnPropertyChanged(nameof(CurrentStatusDate));
            }
        }

        /// <summary>
        /// Not persisted, generated dynamically.
        /// </summary>
        [JsonIgnore]
        public IList<string> CommonTags
        {
            get => _commonTags;
            set
            {
                _commonTags = value;
                OnPropertyChanged(nameof(CommonTags));
            }
        }

        /// <summary>
        /// Not persisted, must be calculated on the fly.
        /// </summary>
        [JsonIgnore]
        public ConnectionQuality ConnectionQuality
        {
            get => _connectionQuality;
            set
            {
                _connectionQuality = value;
                OnPropertyChanged(nameof(ConnectionQuality));
            }
        }

        [JsonIgnore]
        public double ConnectionSpeed
        {
            get => _connectionSpeed;
            set
            {
                _connectionSpeed = value;
                OnPropertyChanged(nameof(ConnectionSpeed));
            }
        }

        #endregion

        #region CTORS

        public Project() 
        {
            this.AutoDownload = true;
            this.Packages = new ObservableCollection<LocalPackage>();
            this.AvailablePackageIds = new List<string>();
            this.CommonTags = new List<string>();
            this.RequiredTags = new string[0];
            this.IgnoreTags = new string[0];
            this.PackageSyncCount = 3;
            this.CurrentStatus = string.Empty;
            this.ConnectionQuality = ConnectionQuality.Untested;
        }

        #endregion

        #region METHODS

        public void AddPackage(LocalPackage package) 
        {
            _addQueue.Add(package);
        }

        public void RemovePackage(string id) 
        {
            _removeQueue.Add(id);
        }

        public void SyncPackages() 
        {
            lock (_packages)
            {
                foreach (LocalPackage package in _addQueue)
                    this.Packages.Add(package);

                _addQueue.Clear();

                foreach (string id in _removeQueue)
                {
                    LocalPackage package = this.Packages.FirstOrDefault(p => p.Package.Id == id);
                    if (package != null)
                        this.Packages.Remove(package);
                }

                _removeQueue.Clear();
            }
        }

        public void SetStatus(string status) 
        {
            _stat = status;
            _statDate = DateTime.Now;
        }

        public void GenerateStatus() 
        {
            if (string.IsNullOrEmpty(_stat) && string.IsNullOrEmpty(_currentStatus))
                return;

            if (string.IsNullOrEmpty(_stat))
            {
                this.CurrentStatus = string.Empty;
                this.CurrentStatusDate = string.Empty;
            }
            else
            {
                this.CurrentStatus = $"{_stat}";
                this.CurrentStatusDate = $" ({(DateTime.Now - _statDate).ToHumanString()} ago)";
            }
        }

        /// <summary>
        /// Populates packages collection from disk
        /// </summary>
        public void PopulatePackageList()
        {
            string localProjectPackagesDirectory = Path.Combine(GlobalDataContext.Instance.ProjectsRootDirectory, this.Id);
            if (!Directory.Exists(localProjectPackagesDirectory))
                return;

            IEnumerable<string> packageIds = Directory.GetDirectories(localProjectPackagesDirectory).
                Select(p => Path.GetFileName(p));

            List<LocalPackage> newPackages = new List<LocalPackage>();
            foreach (string packageId in packageIds)
            {
                if (_rawPackages.Any(p => p.Package.Id == packageId))
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
            newPackages = newPackages
                .OrderByDescending(p => p.Package.CreatedUtc)
                .ToList();

            // before any filtering, peak at all tags, we need thems
            foreach (var package in newPackages)
                foreach (string tag in package.Package.Tags)
                {
                    if (!_rawTags.ContainsKey(tag))
                        _rawTags.Add(tag, 0);

                    _rawTags[tag]++;
                }

            this.CommonTags = _rawTags
                .Where(t => t.Value > 1)
                .Select(t => t.Key)
                .OrderBy(t => t)
                .ToList();

            if (newPackages.Any())
                _rawPackages = _rawPackages.Concat(newPackages);

            IList<LocalPackage> filteredPackages = _rawPackages.ToList();

            // refilter packages based on user prefs
            if (this.RequiredTags.Any())
                filteredPackages = (from package in filteredPackages
                                  where package.Package.Tags.Any()
                                && !package.Package.Tags.Except(this.RequiredTags).Any()
                               select package).ToList();

            if (this.IgnoreTags.Any())
                filteredPackages = (from package in filteredPackages
                                   where package.Package.Tags.Except(IgnoreTags).Any()
                                select package).ToList();

            int count = this.Packages.Count;
            bool sync = false;

            for (int i = 0; i < count; i++)
            {
                string id = this.Packages[count - i - 1].Package.Id;
                if (!filteredPackages.Any(p => p.Package.Id == id))
                {
                    this.RemovePackage(id);
                    sync = true;
                }
            }

            foreach (var filteredPackage in filteredPackages)
                if (!this.Packages.Any(p => p.Package.Id == filteredPackage.Package.Id))
                {
                    this.AddPackage(filteredPackage);
                    sync = true;
                }

            if (sync)
                this.SyncPackages();

            for (int i = 0; i < this.Packages.Count; i++) 
                this.Packages[i].EnableAutoSave();

            System.Diagnostics.Debug.WriteLine($"PopulatePackageList:{DateTime.Now.Second}:{this.Packages.Count}");
        }

        #endregion
    }
}
