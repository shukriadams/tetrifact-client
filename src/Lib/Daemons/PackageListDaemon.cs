using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrifactClient
{
    /// <summary>
    /// Updates package list for all projects
    /// </summary>
    public class PackageListDaemon
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
                        GlobalDataContext.Instance.Console.Add($"Unexpected error getting package manifest : {ex.Message}");
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
            HttpPayloadRequest request = new HttpPayloadRequest(HttpHelper.UrlJoin(new string[] { project.BuildServer, "v1", "packages" } ));
            request.Attempt();

            Project contextProject = GlobalDataContext.Instance.Projects.Projects.FirstOrDefault(p => p.Id == project.Id);

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
                contextProject.ServerErrorDescription = null;
                lock (GlobalDataContext.Instance)
                    contextProject.AvailablePackages = data.Success.Packages.ToList();
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
    }
}
