using System;
using System.Linq;
using System.Threading.Tasks;

namespace TetrifactClient
{
    public class LocalPackageDeleteDaemon : IDaemon
    {
        public void Start()
        {
            DaemonProcessRunner runner = new DaemonProcessRunner();
            Log log = new Log();
            runner.Start(new AsyncDo(this.Work), (int)new TimeSpan(0, 10, 0).TotalMilliseconds, new Log());
        }
        
        public void WorkNow()
        {
            throw new NotImplementedException();
        }

        public async Task Work()
        {
            foreach (Project project in GlobalDataContext.Instance.Projects.Projects)
            {
                foreach (LocalPackage package in project.Packages.Items.Where(package => package.IsMarkedForDelete()))
                {
                    // delete stuff
                    package.TransferState = BuildTransferStates.Deleting;

                    try
                    {
                        package.TransferState = BuildTransferStates.Deleted;
                    }
                    catch (Exception ex)
                    {
                        package.TransferState = BuildTransferStates.DeleteFailed;
                    }
                }
            }
        }
    }
}
