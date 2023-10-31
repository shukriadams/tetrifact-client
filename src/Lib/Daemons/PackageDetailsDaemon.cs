using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TetrifactClient.Models;

namespace TetrifactClient
{
    /// <summary>
    /// Daemon for downloading metadata details of each package from remote server. Doesn't download binaries, only meta data.
    /// </summary>
    public class PackageDetailsDaemon
    {
        public void Start()
        {
            DaemonProcessRunner runner = new DaemonProcessRunner();
            Log log = new Log();
            runner.Start(new AsyncDo(this.Work), GlobalDataContext.Instance.DaemonIntervalMS, new Log());
        }

        public async Task Work()
        {
            foreach (Project project in GlobalDataContext.Instance.Projects.Projects) 
            {
                Project contextProject = GlobalDataContext.Instance.Projects.Projects.FirstOrDefault(p => p.Id == project.Id);

                // todo : project name must be made file-system safe
                string localProjectPackagesDirectory = Path.Combine(GlobalDataContext.Instance.GetProjectsDirectoryPath(), project.Id, "packages");

                foreach (string availablePackage in contextProject.AvailablePackages)
                {
                    string localPackagePath = Path.Combine(localProjectPackagesDirectory, availablePackage, $"base.json");
                    string localPackagePathFiles = Path.Combine(localProjectPackagesDirectory, availablePackage, $"files.json");
                    if (File.Exists(localPackagePath))
                        continue;

                    Directory.CreateDirectory(Path.GetDirectoryName(localPackagePath));

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
                        data.Tags = data.Tags.OrderBy(t => t);

                        File.WriteAllText(localPackagePath, JsonConvert.SerializeObject(data, Formatting.Indented));

                        PackageFiles data2 = JsonConvert.DeserializeObject<PackageFiles>(payload);
                        if (data == null)
                        {
                            // handle error
                            continue;
                        }

                        File.WriteAllText(localPackagePathFiles, JsonConvert.SerializeObject(data2, Formatting.Indented));

                        contextProject.ServerState = SourceServerStates.Normal;
                        contextProject.ServerErrorDescription = null;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(request.Error))
                            contextProject.ServerErrorDescription = request.Error;
                        else
                            contextProject.ServerErrorDescription = "Server unavailable";

                        contextProject.ServerState = SourceServerStates.Unavailable;
                    }
                }

                project.PopulateAvailableProjectsList();
            }


        }
    }
}
