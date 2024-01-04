using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace TetrifactClient
{
    public class PackageRunner
    {
        public VoidDo Exited;
        private void HandleError(Exception ex) 
        {
            Log log = new Log();
            log.LogError(ex);

            // run on main thread, else go boom
            Dispatcher.UIThread.Post(() => {

                Alert alert = new Alert();
                alert.SetContent("Error", ex.ToString());
                alert.ShowDialog(MainWindow.Instance);

            }, DispatcherPriority.Background);
        }

        public void Run(GlobalDataContext context, LocalPackage package, Project project) 
        {
            try
            {
                ProcessStartInfo gameProcessStartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(PathHelper.ToUnixPath(PathHelper.GetPackageContentDirectoryPath(context, project, package)), project.ApplicationExecutableName),
                    WorkingDirectory = PathHelper.ToUnixPath(PathHelper.GetPackageContentDirectoryPath(context, project, package))
                };

                Process process = new Process
                {
                    StartInfo = gameProcessStartInfo,
                    EnableRaisingEvents = true
                };

                process.Exited += (object sender, EventArgs e) =>
                {
                    if (this.Exited != null)
                        this.Exited.Invoke();
                };

                Thread thread = new Thread(new ThreadStart(delegate ()
                {
                    try
                    {
                        process.Start();

                    }
                    catch (Exception ex) 
                    {
                        this.HandleError(ex);
                    }
                }));

                thread.Start();

            }
            catch (Exception ex) 
            {
                this.HandleError(ex);
            }
        }
    }
}
