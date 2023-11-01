using System;
using System.Linq;
using System.Threading.Tasks;

namespace TetrifactClient.Lib.Daemons
{
    public class LocalPackageDeleteDaemon
    {
        public void Start()
        {
            DaemonProcessRunner runner = new DaemonProcessRunner();
            Log log = new Log();
            runner.Start(new AsyncDo(this.Work), (int)new TimeSpan(0, 10, 0).TotalMilliseconds, new Log());
        }

        public async Task Work()
        {
            foreach (Project project in GlobalDataContext.Instance.Projects.Projects)
            {
                foreach (Package package in project.Packages.Where(package => package.LocalPackage.IsMarkedForDelete()))
                {
                    // delete stuff
                    package.LocalPackage.TransferState = BuildTransferStates.Deleting;

                    try
                    {
                        package.LocalPackage.TransferState = BuildTransferStates.Deleted;
                    }
                    catch (Exception ex)
                    {
                        package.LocalPackage.TransferState = BuildTransferStates.DeleteFailed;
                    }
                }
            }
        }
    }
}
