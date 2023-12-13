using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TetrifactClient
{
    /// <summary>
    /// Deletes packages that have been marked for delete, as well as hanging downloads
    /// </summary>
    public class PackageMarkForDeleteDaemon : IDaemon
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
                IList<LocalPackage> cloned = project.Packages.Where(package => package.IsMarkedForDeleteOrBeingDeleting()).ToList();

                for (int i = 0; i < cloned.Count; i ++ )
                {
                    LocalPackage packageMarkedForDelete = cloned[i];

                    string packageDirectory = Path.Combine(context.ProjectsRootDirectory, project.Id, packageMarkedForDelete.Package.Id, "_");
                    string deletePath = Path.Combine(context.ProjectsRootDirectory, project.Id, packageMarkedForDelete.Package.Id, "!_");

                    if (Directory.Exists(packageDirectory) && !Directory.Exists(deletePath))
                    {
                        try
                        {
                            Directory.Move(packageDirectory, deletePath);
                        }
                        catch (Exception)
                        {
                            // ignore errors here, they will be caused by access locks
                            continue;
                        }
                    }

                    if (!Directory.Exists(packageDirectory) && !Directory.Exists(deletePath))
                    {
                        packageMarkedForDelete.TransferState = PackageTransferStates.Deleted;
                        packageMarkedForDelete.DownloadProgress.Message = "Deleted";
                    }
                }
            }
        }
    }
}
