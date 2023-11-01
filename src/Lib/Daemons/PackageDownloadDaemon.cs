using System.Linq;
using System.Threading.Tasks;

namespace TetrifactClient
{
    public class PackageDownloadDaemon : IDaemon
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
                foreach (Package package in project.Packages.Where(package => package.LocalPackage.IsQueuedForDownload()))
                {
                    package.LocalPackage.Download();
                }
            }
        }
    }
}
