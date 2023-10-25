using DynamicData;
using DynamicData.Binding;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TetrifactClient
{
    public class GlobalDataContextSerialize 
    {
        public IEnumerable<Project> Projects { get; set; }

        public string DataFolder { get; set; }
    }

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

        public GlobalDataContext() 
        {
            // default
            this.DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetExecutingAssembly().GetName().Name);
        }

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

            if (!File.Exists(filePath))
                return;
            
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

            _instance.ProjectTemplates.Projects = ResourceLoader.DeserializeFromJson<ObservableCollection<Project>>("Templates.Projects.json");

            // load packages for each project

            foreach (Project project in _instance.Projects.Projects) 
            {
                string localProjectPackagesDirectory = Path.Combine(GlobalDataContext.Instance.GetProjectsDirectoryPath(), project.Name, "packages");
                if (!Directory.Exists(localProjectPackagesDirectory))
                    continue;

                IEnumerable<string> packages = Directory.
                    GetDirectories(localProjectPackagesDirectory).
                    Select(p => Path.GetFileName(p));

                foreach (string package in packages) 
                {
                    if (project.Packages.Any(p => p.Id == package))
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

                        project.Packages.Add(packageObject);

                    } 
                    catch (Exception ex)
                    {
                        // todo : handle
                        throw;
                    }
                }
            }

            _instance.Projects.Projects.ToObservableChangeSet(t => t.Name)
                .Subscribe(t => {
                    Save();
                });
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
