using System;
using System.Diagnostics;
using System.Threading;

namespace TetrifactClient
{
    public class PackageRunner
    {
        public VoidDo Exited;

        public void Run(GlobalDataContext context, LocalPackage package, Project project) 
        {
            ProcessStartInfo gameProcessStartInfo = new ProcessStartInfo
            {
                FileName = project.ApplicationExecutableName,
                WorkingDirectory = PathHelper.GetPackageDirectoryPath(context, project, package)
            };

            Process process = new Process
            {
                StartInfo = gameProcessStartInfo,
                EnableRaisingEvents = true
            };

            process.Exited += (object sender, EventArgs e) =>{
                if (this.Exited != null)
                    this.Exited.Invoke();
            };

            Thread thread = new Thread(new ThreadStart(delegate () {
                process.Start();
            }));

            thread.Start();
        }
    }
}
