using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrifactClient
{
    public class PackageDetailsDaemon
    {
        private bool _busy;
        private int _delay = 5000;

        public void Start()
        {
            Task.Run(async () => {
                while (true)
                {
                    try
                    {
                        if (_busy)
                            continue;

                        _busy = true;

                        foreach (Project project in GlobalDataContext.Instance.Projects.Projects)
                            await this.Work(project);
                    }
                    catch (Exception ex)
                    {
                        GlobalDataContext.Instance.Console.Items.Add($"Unexpected error getting package manifest : {ex.Message}");
                        // todo : write ex to log file
                    }
                    finally
                    {
                        _busy = false;

                        // force thread pause so this loop doesn't lock CPU
                        await Task.Delay(_delay);
                    }
                } // while
            });
        }

        private async Task Work(Project project)
        {
            Project contextProject = GlobalDataContext.Instance.Projects.Projects.FirstOrDefault(p => p.Name == project.Name);

            string localProjectPackagesDirectory = Path.Combine(GlobalDataContext.Instance.GetProjectsDirectoryPath(), project.Name, "packages");
            Directory.CreateDirectory(localProjectPackagesDirectory);

            foreach (string availablePackage in contextProject.AvailablePackages)
            {
                string localPackagePath = Path.Combine(localProjectPackagesDirectory, availablePackage);
                if (File.Exists(localPackagePath))
                    continue;

                HttpLocalFileRequest request = new HttpLocalFileRequest(new Uri(new Uri(localPackagePath), availablePackage), localPackagePath);
                request.Attempt();

                if (!request.Succeeded)
                {
                    if (!string.IsNullOrEmpty(request.Error))
                        contextProject.ServerErrorDescription = request.Error;

                    contextProject.ServerState = SourceServerStates.Unavailable;
                }
            }
        }
    }
}
