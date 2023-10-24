using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using TetrifactClient.Models;

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
    
            // todo : project name must be made file-system safe
            string localProjectPackagesDirectory = Path.Combine(GlobalDataContext.Instance.GetProjectsDirectoryPath(), project.Name, "packages");
            Directory.CreateDirectory(localProjectPackagesDirectory);

            foreach (string availablePackage in contextProject.AvailablePackages)
            {
                string localPackagePath = Path.Combine(localProjectPackagesDirectory, $"{availablePackage}.json");
                string localPackagePathFiles = Path.Combine(localProjectPackagesDirectory, $"{availablePackage}-files.json");
                if (File.Exists(localPackagePath))
                    continue;

                string url = HttpHelper.UrlJoin(new string[] { project.BuildServer, "v1", "packages", availablePackage });
                HttpPayloadRequest request = new HttpPayloadRequest(url);
                request.Attempt();
                if (request.Succeeded)
                {
                    string payload = Encoding.Default.GetString(request.Payload);
                    Package data = JsonConvert.DeserializeObject<Package>(payload);
                    if (data == null)
                    {
                        // handle error
                        continue;
                    }

                    File.WriteAllText(localPackagePath, JsonConvert.SerializeObject(data));
                    PackageFiles data2 = JsonConvert.DeserializeObject<PackageFiles>(payload);
                    if (data == null)
                    {
                        // handle error
                        continue;
                    }

                    File.WriteAllText(localPackagePathFiles, JsonConvert.SerializeObject(data2));

                    contextProject.ServerState = SourceServerStates.Normal;
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
}
