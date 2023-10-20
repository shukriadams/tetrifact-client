using DynamicData;
using DynamicData.Binding;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace TetrifactClient
{
    public class GlobalDataContextSerialize 
    {
        public IEnumerable<Project> Projects { get; set; }
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

        public static GlobalDataContext Instance 
        { 
            get 
            {
                if (_instance == null)
                {
                    _instance = new GlobalDataContext();
                    
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.json");
                    if (File.Exists(filePath))
                    {
                        string rawJson = null;
                        try
                        {
                            rawJson = File.ReadAllText(filePath);
                        }
                        catch (Exception ex)
                        {
                            // handle error, for now rethrow
                            throw;
                        }

                        GlobalDataContextSerialize settings = null;

                        try
                        {
                            settings = JsonConvert.DeserializeObject<GlobalDataContextSerialize>(rawJson);

                        }
                        catch (Exception ex) 
                        {
                            // handle error, for now rethrow
                            // settings corrupt, consider deleting
                            throw;
                        }

                        Instance.Projects.Projects.AddRange(settings.Projects);
                    }

                    _instance.ProjectTemplates.Projects = ResourceLoader.DeserializeFromJson<ObservableCollection<Project>>("Templates.Projects.json");
    
                    _instance.Projects.Projects.ToObservableChangeSet(t => t.Name)
                      .Subscribe(t => {
                          Save();
                      });

                    _instance.Console.Items.Add("loaded!");
                }

                return _instance;
            } 
        }

        #endregion

        #region METHODS

        public static void Save()
        {
            GlobalDataContextSerialize serialize = new GlobalDataContextSerialize();
            serialize.Projects = _instance.Projects.Projects;

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.json");
            File.WriteAllText(filePath, JsonConvert.SerializeObject(serialize, Formatting.Indented ));
        }

        #endregion
    }
}
