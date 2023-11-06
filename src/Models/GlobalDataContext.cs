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

        private static GlobalDataContext _instance;
        private string caption = "some text";
        private Project _focusedProject;

        #endregion

        #region PROPERTIES

        public IEnumerable<SourceServer> SourceServers { get; } = new List<SourceServer>();

        public ProjectsViewModel Projects { get; } = new ProjectsViewModel();
        
        public ProjectsViewModel ProjectTemplates { get; } = new ProjectsViewModel();

        public ConsoleViewModel Console { get; } = new ConsoleViewModel();

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

        public string DataFolder { get; set; }
 
        public int DaemonIntervalMS { get; set; }

        public static GlobalDataContext Instance 
        { 
            get 
            {
                if (_instance == null)
                {
                    _instance = new GlobalDataContext();
                    Load();
                }

                return _instance;
            } 
        }

        #endregion

        #region CTORS

        public GlobalDataContext() 
        {
            // default
            this.DataFolder = PathHelper.GetInternalDirectory();
            this.DaemonIntervalMS = 5000;
        }

        #endregion

        #region METHODS

        public string GetProjectsDirectoryPath() 
        {
            return Path.Combine(this.DataFolder, "projects");
        }

        public static void Load() 
        {
            string filePath = Path.Combine(_instance.DataFolder, "Settings.json");
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

                if (persistedSettings.DataFolder != null)
                    Instance.DataFolder = persistedSettings.DataFolder;

            }

            _instance.ProjectTemplates.Projects = ResourceLoader.DeserializeFromJson<ObservableCollection<Project>>("Templates.Projects.json");

            // load packages for each project
            foreach (Project project in _instance.Projects.Projects) 
                project.PopulateProjectsList();

            // set up event to save config to disk whenever changed
            _instance.Projects.Projects.ToObservableChangeSet(t => t.Id)
                .Subscribe(t => {
                    Save();
                });

            if (_instance.Projects.Projects.Any())
                _instance.FocusedProject = _instance.Projects.Projects.First();
        }

        public static void Save()
        {
            GlobalDataContextSerialize serialize = new GlobalDataContextSerialize();
            serialize.Projects = _instance.Projects.Projects;

            string filePath = Path.Combine(_instance.DataFolder, "Settings.json");
            File.WriteAllText(filePath, JsonConvert.SerializeObject(serialize, Formatting.Indented ));
        }

        #endregion
    }
}
