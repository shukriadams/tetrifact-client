using Avalonia.Controls;
using System.Threading.Tasks;

namespace TetrifactClient
{
    public partial class SettingsForm : Window
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void OnPathSelect(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                OpenFolderDialog dialog = new OpenFolderDialog();
                string path = await dialog.ShowAsync(this);
                if (!string.IsNullOrEmpty(path))
                    GlobalDataContext.Instance.ProjectsRootDirectory = path;
            });
        }
    }
}
