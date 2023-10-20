using Avalonia.Controls;
using Avalonia.Interactivity;
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
    }
}
