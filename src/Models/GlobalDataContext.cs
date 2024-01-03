using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TetrifactClient
{
    public class GlobalDataContext : ReactiveObject
    {
        #region FIELDS

        private static GlobalDataContext _thisInstance;
        private string caption = "some text";
        private Project _focusedProject;
        private string _projectsRootDirectory;

        #endregion

        #region PROPERTIES

        public IEnumerable<SourceServer> SourceServers { get; } = new List<SourceServer>();

        public ProjectsViewModel Projects { get; } = new ProjectsViewModel();
        
        public ProjectsViewModel ProjectTemplates { get; } = new ProjectsViewModel();

        public ConsoleViewModel Console { get; } = new ConsoleViewModel();

        public Preferences Preferences { get; } = new Preferences();

        public LogLevels LogLevel { get; set; }

        public Project FocusedProject
        {
            get => _focusedProject;
            set => this.RaiseAndSetIfChanged(ref _focusedProject, value);
        }

        public string Caption
        {
            get => caption;
            set => caption = value;
        }

        [JsonIgnore]
        public string DataFolder { get; set; }
 
        public int DaemonIntervalMS { get; set; }

        public static GlobalDataContext Instance 
        { 
            get 
            {
                if (_thisInstance == null)
                {
                    _thisInstance = new GlobalDataContext();
                    Load();
                }

                return _thisInstance;
            } 
        }

        /// <summary>
        /// Milliseconds
        /// </summary>
        public int Timeout { get; set; }

        public int ThreadLoad { get; set; }

        public string ProjectsRootDirectory
        {
            get => _projectsRootDirectory;
            set => this.RaiseAndSetIfChanged(ref _projectsRootDirectory, value);
        }

        #endregion

        #region CTORS

        public GlobalDataContext() 
        {
            // core data folder is always in appdata/local/<assembly name>
            this.DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetExecutingAssembly().GetName().Name);
            
            // default projects path where projects + packages are stored. user-definable
            this.ProjectsRootDirectory = Path.Combine(this.DataFolder, "projects");
            this.DaemonIntervalMS = 5000;
            this.Timeout = 3000;
            this.ThreadLoad = 4;
        }

        #endregion

        #region METHODS

        public static void Load() 
        {
            string filePath = Path.Combine(_thisInstance.DataFolder, "Settings.json");
            string rawJson = null;
            GlobalDataContextSerialize persistedSettings = null;

            if (File.Exists(filePath)) 
            {

                try
                {
                    rawJson = File.ReadAllText(filePath);
                }
                catch (Exception ex)
                {
                    // handle error, for now rethrow
                    throw;
                }

                try
                {
                    persistedSettings = JsonConvert.DeserializeObject<GlobalDataContextSerialize>(rawJson);
                }
                catch (Exception ex)
                {
                    // handle error, for now rethrow
                    // settings corrupt, consider deleting
                    throw;
                }

                if (persistedSettings.Projects != null)
                    Instance.Projects.Projects.AddRange(persistedSettings.Projects);

                if (persistedSettings.ProjectsRootDirectory!= null)
                    Instance.ProjectsRootDirectory = persistedSettings.ProjectsRootDirectory;

                Instance.Timeout = persistedSettings.Timeout;

            }

            _thisInstance.ProjectTemplates.Projects = ResourceLoader.DeserializeFromJson<ObservableCollection<Project>>("Templates.Projects.json");

            // load packages for each project
            foreach (Project project in _thisInstance.Projects.Projects)
            {
                //project.PopulatePackageList();
            }

            Dispatcher.UIThread.Post(() => {

            },
            DispatcherPriority.Background);


            // set up event to save config to disk whenever changed
            _thisInstance.Projects.Projects.ToObservableChangeSet(t => t.Id)
                .Subscribe(t => {
                    Save();
                });

            if (_thisInstance.Projects.Projects.Any())
                _thisInstance.FocusedProject = _thisInstance.Projects.Projects.First();
        }

        public static void Save()
        {
            GlobalDataContextSerialize serialize = new GlobalDataContextSerialize();

            // manually copy over the stuff we want to serialize
            serialize.Projects = _thisInstance.Projects.Projects;
            serialize.ProjectsRootDirectory = _thisInstance.ProjectsRootDirectory;
            serialize.Timeout = _thisInstance.Timeout;

            string filePath = Path.Combine(_thisInstance.DataFolder, "Settings.json");
            var settings = new JsonSerializerSettings();
            //settings.i
            string json = JsonConvert.SerializeObject(serialize, Formatting.Indented, settings);
            
            try
            {
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                _thisInstance.Console.Add(ex.ToString());
            }
                
        }

        #endregion
    }
}
