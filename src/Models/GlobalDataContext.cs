using DynamicData.Binding;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace TetrifactClient
{
    public class GlobalDataContext
    {
        private static GlobalDataContext _instance;

        public IEnumerable<SourceServer> SourceServers { get; } = new List<SourceServer>();

        public ProjectsViewModel Projects { get; } = new ProjectsViewModel();
        public ProjectsViewModel ProjectTemplates { get; } = new ProjectsViewModel();

        private string caption = "some text";

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
                    _instance.ProjectTemplates.Projects = ResourceLoader.DeserializeFromJson<ObservableCollection<Project>>("Templates.Projects.json");

                    _instance.Projects.Projects.ToObservableChangeSet(t => t.Name)
                      .Subscribe(t => {
                          string test = "";
                      });


                    _instance.Projects.WhenAnyValue(vm => vm.Projects)
                      .Subscribe(t => {
                          string test = "";
                      });

                }
                return _instance;

            } 
        }

        private static void Projects_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.json");
            File.WriteAllText(filePath, JsonConvert.SerializeObject(_instance));
        }
    }
}
