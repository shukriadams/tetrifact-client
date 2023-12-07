using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrifactClient
{
    /// <summary>
    /// Daemon for downloading metadata details of each package from remote server. Doesn't download binaries, only meta data.
    /// </summary>
    public class PackageDetailsDaemon : IDaemon
    {
        public void Start()
        {
            DaemonProcessRunner runner = new DaemonProcessRunner();
            Log log = new Log();
            runner.Start(new AsyncDo(this.Work), GlobalDataContext.Instance.DaemonIntervalMS, new Log());
        }

        public void WorkNow()
        {
            throw new NotImplementedException();
        }

        public async Task Work()
        {

            foreach (Project project in GlobalDataContext.Instance.Projects.Projects) 
            {
                Project contextProject = GlobalDataContext.Instance.Projects.Projects.FirstOrDefault(p => p.Id == project.Id);

                // todo : project name must be made file-system safe
                string localProjectPackagesDirectory = Path.Combine(GlobalDataContext.Instance.GetProjectsDirectoryPath(), project.Id, "packages");

                foreach (string availablePackage in contextProject.AvailablePackageIds)
                {
                    string packageHeadPath = Path.Combine(localProjectPackagesDirectory, availablePackage, $"remote.json");
                    string packageFilesPath = Path.Combine(localProjectPackagesDirectory, availablePackage, $"files.json");

                    if (File.Exists(packageHeadPath))
                        continue;

                    contextProject.SetStatus($"Querying server for new package {availablePackage}");
                    Directory.CreateDirectory(Path.GetDirectoryName(packageHeadPath));

                    string url = HttpHelper.UrlJoin(new string[] { project.TetrifactServerAddress, "v1", "packages", availablePackage });
                    HttpPayloadRequest httpRequest = new HttpPayloadRequest(url);
                    httpRequest.Attempt();

                    if (httpRequest.Succeeded)
                    {
                        string payload = Encoding.Default.GetString(httpRequest.Payload);
                        dynamic payloadDynamic = JsonConvert.DeserializeObject(payload);
                        if (payloadDynamic == null || payloadDynamic.success == null)
                            throw new Exception($"Received error response : {payload}");

                        Package package = JsonConvert.DeserializeObject<Package>(payloadDynamic.success.package.ToString());
                        if (package == null)
                        {
                            // handle error
                            continue;
                        }

                        // write package files to own file
                        File.WriteAllText(packageFilesPath, JsonConvert.SerializeObject(package.Files, Formatting.Indented));
                        
                        // empty package files before writing to "head" file
                        package.Files = new PackageFile[0];

                        // force tags to be alpbateically ordered
                        package.Tags = package.Tags.OrderBy(t => t);

                        // package remote data in local data parent
                        LocalPackage localPackage = new LocalPackage();
                        localPackage.TetrifactServerAddress = project.TetrifactServerAddress;
                        localPackage.Package = package;

                        File.WriteAllText(packageHeadPath, JsonConvert.SerializeObject(localPackage, Formatting.Indented));

                        contextProject.ServerState = SourceServerStates.Normal;
                        contextProject.ServerErrorDescription = null;
                        contextProject.SetStatus($"Downloaded info for package {availablePackage}");
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(httpRequest.Error))
                            contextProject.ServerErrorDescription = httpRequest.Error;
                        else
                            contextProject.ServerErrorDescription = "Server unavailable";

                        contextProject.ServerState = SourceServerStates.Unavailable;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"PackageDetailsDaemon:WORK {DateTime.Now.Second} project:{project.Name}");
               // project.PopulatePackageList();
            }
        }
    }
}
