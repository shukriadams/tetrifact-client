using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrifactClient
{
    /// <summary>
    /// Gets a list of remote packages available. Packages are listed, not downloaded.
    /// </summary>
    public class PackageRemoteListDaemon : IDaemon
    {
        private bool _busy;
        private int _delay = 5000;
        private ILog _log;

        public void Start()
        {
            DaemonProcessRunner runner = new DaemonProcessRunner();
            _log = new Log();
            
            runner.Start(new AsyncDo(this.Work), (int)new TimeSpan(0, 0, 5).TotalMilliseconds, new Log());
        }

        public void WorkNow()
        {
            throw new NotImplementedException();
        }

        private async Task Work()
        {
            foreach (Project project in GlobalDataContext.Instance.Projects.Projects) 
            {
                Project contextProject = GlobalDataContext.Instance.Projects.Projects.FirstOrDefault(p => p.Id == project.Id);
                contextProject.SetStatus("Checking server for new packages");

                HttpPayloadRequest request = new HttpPayloadRequest(HttpHelper.UrlJoin(new string[] { project.TetrifactServerAddress, "v1", "packages" }));
                request.Attempt();

                if (request.Succeeded)
                {
                    // somehow project was deleted since call started, this is an edge case and can be ignored
                    if (contextProject == null)
                        return;

                    string payload = Encoding.Default.GetString(request.Payload);
                    dynamic payloadDynamic = JsonConvert.DeserializeObject(payload);
                    if (payloadDynamic == null || payloadDynamic.success == null) 
                    {
                        contextProject.SetStatus("Error getting list of packages, check log");
                        _log.LogUnstability($"Received error response : {payload}");
                        continue;
                    }

                    IEnumerable<string> packageIds = JsonConvert.DeserializeObject<IEnumerable<string>>(payloadDynamic.success.packages.ToString());

                    contextProject.ServerState = SourceServerStates.Normal;
                    contextProject.ServerErrorDescription = null;
                    lock (GlobalDataContext.Instance)
                        contextProject.AvailablePackageIds = packageIds.ToList();
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
}
