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

        private void OnProjectClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            GlobalDataContext.Instance.FocusedProject = GlobalDataContext.Instance.Projects.Projects.FirstOrDefault(p => p.Name == button.Content);
        }

        private void listProjects_Clicked(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            string text = null;
            object source = e.Source;

            if (e.Source is ContentPresenter)
                source = ((ContentPresenter)e.Source).Child;

            if (source is TextBlock)
                text = ((TextBlock)source).Text;

            if (text == null)
                return;

            GlobalDataContext.Instance.FocusedProject = GlobalDataContext.Instance.Projects.Projects.FirstOrDefault(p => p.Name == text);
        }
    }
}
