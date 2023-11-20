using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using System.Collections.Specialized;
using System.Linq;
using Unity;

namespace TetrifactClient
{
    public partial class ProjectsView : UserControl
    {
        public ProjectsView()
        {
            InitializeComponent();

            listProjects.Items.CollectionChanged += (object? sender, NotifyCollectionChangedEventArgs e) =>{
                // auto focus first item in list
                if (listProjects.Items.Any())
                    listProjects.SelectedIndex = 0;
            };
        }

        public void OnNewProject(object? sender, RoutedEventArgs args)
        {
            ProjectEditorView editor = App.UnityContainer.Resolve<ProjectEditorView>();
            editor.SetProject(null);
            editor.ShowDialog(MainWindow.Instance);
        }

        private void ProjectsList_Clicked(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            string id = null;
            object source = e.Source;

            if (e.Source is ContentPresenter)
                source = ((ContentPresenter)e.Source).Child;

            if (source is TextBlock)
                id = ((TextBlock)source).Tag.ToString();

            if (id == null)
                return;

            GlobalDataContext.Instance.FocusedProject = GlobalDataContext.Instance.Projects.Projects.FirstOrDefault(p => p.Id == id);
        }
    }
}
