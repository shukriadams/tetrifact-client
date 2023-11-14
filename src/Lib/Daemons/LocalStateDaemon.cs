using System;
using System.Threading.Tasks;

namespace TetrifactClient
{
    /// <summary>
    /// 
    /// </summary>
    public class LocalStateDaemon : IDaemon
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

            }
        }
    }
}
