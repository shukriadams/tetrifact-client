using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TetrifactClient
{
    public class PackageDeleteDaemon : IDaemon
    {
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

        public async Task Work()
        {
            GlobalDataContext context = GlobalDataContext.Instance;

            foreach (Project project in GlobalDataContext.Instance.Projects.Projects)
            {
                string projectDirectory = PathHelper.GetProjectDirectoryPath(GlobalDataContext.Instance, project);
                if (!Directory.Exists(projectDirectory))
                    continue;

                string[] packagedirectories = Directory.GetDirectories(projectDirectory);

                foreach (string packagedirectory in packagedirectories)
                {
                    IEnumerable<string> markedDirectories = Directory.GetDirectories(packagedirectory)
                        .Where(dir => Path.GetFileName(dir).StartsWith("!"));

                    foreach (string markedDirectory in markedDirectories) 
                    {
                        try
                        {
                            // delete stuff
                            Directory.Delete(markedDirectory, true);
                        }
                        catch (Exception ex)
                        {
                            _log.LogError(ex);
                        }
                    }
                }
            }
        }
    }
}
