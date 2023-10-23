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
            HttpPayloadRequest request = new HttpPayloadRequest(project.BuildServer);
            request.Attempt();

            Project contextProject = GlobalDataContext.Instance.Projects.Projects.FirstOrDefault(p => p.Name == project.Name);

            string localProjectPackagesDirectory = Path.Combine(GlobalDataContext.Instance.GetProjectsDirectoryPath(), project.Name, "packages");
            Directory.CreateDirectory(localProjectPackagesDirectory);

            foreach (string availablePackage in contextProject.AvailablePackages)
            {
                string localPackagePath = Path.Combine(localProjectPackagesDirectory, availablePackage);
                if (File.Exists(localPackagePath))
                    continue;

                HttpLocalFileRequest localLocalFileRequest = new HttpLocalFileRequest(new Uri(localPackagePath, ));
            }

            if (request.Succeeded)
            {
                // somehow project was deleted since call started, this is an edge case and can be ignored
                if (contextProject == null)
                    return;

                string payload = Encoding.Default.GetString(request.Payload);
                Payload<PackagesLookup> data = JsonConvert.DeserializeObject<Payload<PackagesLookup>>(payload);
                if (data == null || !string.IsNullOrEmpty(data.Error))
                {
                    // handle error
                    return;
                }

                contextProject.ServerState = SourceServerStates.Normal;
                lock (GlobalDataContext.Instance)
                    contextProject.AvailablePackages = data.Success.Packages;
            }
            else
            {
                if (!string.IsNullOrEmpty(request.Error))
                    contextProject.ServerErrorDescription = request.Error;

                contextProject.ServerState = SourceServerStates.Unavailable;
            }

        }
    }
}
