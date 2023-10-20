﻿using ReactiveUI;
using System;
using System.Collections.ObjectModel;

namespace TetrifactClient
{
    [Serializable]
    public class ProjectsViewModel : ReactiveObject
    {
        private ObservableCollection<Project> _projects = new ObservableCollection<Project> { };

        public ObservableCollection<Project> Projects
        {
            get => _projects;
            set => this.RaiseAndSetIfChanged(ref _projects, value);
        }
    }
}
