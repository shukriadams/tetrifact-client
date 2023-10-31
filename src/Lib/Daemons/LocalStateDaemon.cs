using System.Threading.Tasks;

namespace TetrifactClient
{
    /// <summary>
    /// 
    /// </summary>
    public class LocalStateDaemon
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

            }
        }
    }
}
