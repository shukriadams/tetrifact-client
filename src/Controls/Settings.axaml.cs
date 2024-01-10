using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Interactivity;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace TetrifactClient.Controls
{
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();

            txtSavePath.Text = GlobalDataContext.Instance.ProjectsRootDirectory;
        }

        private void On_View_Data_In_Explorer(object? sender, Avalonia.Interactivity.RoutedEventArgs e) 
        {
            string path = GlobalDataContext.Instance.DataFolder;
            if (!Directory.Exists(path))
            {
                GlobalDataContext.Instance.Console.Add($"Expected app data folder {path} does not exist.");
            }

            try
            {
                // note : this is windows specific, need to find a cross-os friendly version 
                
                Process.Start("explorer.exe", path);
            }
            catch (Exception ex)
            {
                Log log = new Log();
                log.LogError(ex, $"Error opening path {path}");
            }
        }
        
        private void OnCloseClick(object? sernder, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnPathSelect(object? sender, RoutedEventArgs e)
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
