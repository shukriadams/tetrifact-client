using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using System.Linq;
using Unity;

namespace TetrifactClient
{
    public partial class ProjectsView : UserControl
    {
        public ProjectsView()
        {
            InitializeComponent();
        }

        public void OnNewProject(object? sender, RoutedEventArgs args)
        {
            ProjectEditorView settingsEditor = App.UnityContainer.Resolve<ProjectEditorView>();
            settingsEditor.DataContext = GlobalDataContext.Instance;
            settingsEditor.ShowDialog(MainWindow.Instance);
        }

        private void listProjects_Clicked(object? sender, Avalonia.Input.TappedEventArgs e)
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
