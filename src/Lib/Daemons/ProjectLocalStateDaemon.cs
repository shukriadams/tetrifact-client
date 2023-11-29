using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace TetrifactClient
{
    /// <summary>
    /// Daemon to force reading of local state for each project from disk.
    /// </summary>
    public class ProjectLocalStateDaemon : IDaemon
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
            // run on main thread else boom
            Dispatcher.UIThread.Post(() => {
                foreach (Project project in GlobalDataContext.Instance.Projects.Projects)
                    project.PopulatePackageList();
            }, DispatcherPriority.Background);
        }
    }
}

