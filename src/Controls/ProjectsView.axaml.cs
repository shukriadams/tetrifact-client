using Avalonia.Controls;
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

        private void OnProjectClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            GlobalDataContext.Instance.FocusedProject = GlobalDataContext.Instance.Projects.Projects.FirstOrDefault(p => p.Name == button.Content);
        }
    }
}
