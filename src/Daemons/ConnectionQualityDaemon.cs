using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace TetrifactClient.Daemons
{
    public class ConnectionQualityDaemon : IDaemon
    {
        private ILog _log;

        Dictionary <Project, bool> _threads = new Dictionary<Project, bool> ();

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
                try
                {
                    if (!_threads.ContainsKey(project))
                        _threads[project] = false;

                    if (_threads[project] == true)
                        continue;

                    _threads[project] = true;

                    // get target file size
                    WebRequest webRequest = HttpWebRequest.Create(project.TetrifactServerAddress);
                    webRequest.Timeout = 10000;
                    DateTime start = DateTime.Now;

                    using (WebResponse webResponse = webRequest.GetResponse())
                    {
                        TimeSpan delay = DateTime.Now - start;
                        if (delay.TotalSeconds <= 1)
                            project.ConnectionQuality = ConnectionQuality.Good;
                        else
                            project.ConnectionQuality = ConnectionQuality.Degraded;

                        project.ConnectionSpeed = delay.TotalSeconds;
                    }
                }
                catch (Exception ex)
                {
                    project.ConnectionQuality = ConnectionQuality.Broken;
                }
                finally 
                {
                    _threads[project] = false;
                }
            }
        }
    }
}
