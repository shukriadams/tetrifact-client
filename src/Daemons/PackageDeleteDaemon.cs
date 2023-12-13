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
            foreach (Project project in GlobalDataContext.Instance.Projects.Projects)
            {
                foreach (LocalPackage package in project.Packages)
                {
                    string markedDirectory = Path.Join(GlobalDataContext.Instance.ProjectsRootDirectory, project.Id, package.Package.Id, "!_");
                    if (!Directory.Exists(markedDirectory))
                        continue;

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
