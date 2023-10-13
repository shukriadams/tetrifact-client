using ReactiveUI;
using System.Collections.Generic;

namespace TetrifactClient
{
    public class ProjectsViewModel : ReactiveObject
    {
        private IEnumerable<Project> _projects = new List<Project> { new Project { Name= "test" } };

        public IEnumerable<Project> Projects
        {
            get => _projects;
            set => this.RaiseAndSetIfChanged(ref _projects, value);
        }
    }
}
