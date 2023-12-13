using System;
using System.Threading.Tasks;

namespace TetrifactClient
{
    public class ProjectCurrentStatusDaemon : IDaemon
    {
        private int _delayMilliseconds = 1500;

        public void Start()
        {
            DaemonProcessRunner runner = new DaemonProcessRunner();
            runner.Start(new AsyncDo(this.Work), (int)new TimeSpan(0, 0, 0, 0, _delayMilliseconds).TotalMilliseconds, new Log());
        }

        public void WorkNow()
        {
            throw new NotImplementedException();
        }

        private async Task Work()
        {
            foreach (Project project in GlobalDataContext.Instance.Projects.Projects)
                project.GenerateStatus();
        }
    }
}
