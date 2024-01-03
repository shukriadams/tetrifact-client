using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TetrifactClient
{
    public partial class SettingsForm : Window
    {
        public SettingsForm()
        {
            InitializeComponent();

            txtSavePath.Text = GlobalDataContext.Instance.ProjectsRootDirectory;
        }

        private void OnPathSelect(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                OpenFolderDialog dialog = new OpenFolderDialog();
                string path = await dialog.ShowAsync(this);

                Dispatcher.UIThread.Post(() => {
                    if (!string.IsNullOrEmpty(path))
                    {
                        // prevent setting path to drive route
                        if (Directory.GetParent(path) == null)
                        {
                            try
                            {
                                Alert alert = new Alert();
                                alert.SetContent("Error", "Storage path cannot be drive root");
                                alert.Show();
                                return;
                            }
                            catch (Exception ex)
                            {
                                string test = ex.ToString();
                            }
                        }

                        GlobalDataContext.Instance.ProjectsRootDirectory = path;
                        GlobalDataContext.Save();
                    }
                }, DispatcherPriority.Background);

            });
        }
    }
}
