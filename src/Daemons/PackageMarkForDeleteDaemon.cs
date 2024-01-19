using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TetrifactClient
{
    /// <summary>
    /// Internally marks directories for delete if package objects are marked for delete. Actual deleting of files is done by the PackageDeleteDaemon.
    /// </summary>
    public class PackageMarkForDeleteDaemon : IDaemon
    {
        private ILog _log;

        public void Start()
        {
            DaemonProcessRunner runner = new DaemonProcessRunner();
            _log = new Log();
            runner.Start(new AsyncDo(this.Work), (int)new TimeSpan(0, 0, 1).TotalMilliseconds, new Log());
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
                // clone to prevent cross-thread conflicts
                IList<LocalPackage> markedForDeleteCloned = project.Packages.Where(package => package.IsMarkedForDeleteOrBeingDeleting()).ToList();

                for (int i = 0; i < markedForDeleteCloned.Count; i ++ )
                {
                    LocalPackage packageMarkedForDelete = markedForDeleteCloned[i];
                    LocalPackage originalPackageInstance = project.Packages.SingleOrDefault(p => p.Package.Id == packageMarkedForDelete.Package.Id);

                    string packageDirectory = Path.Combine(context.ProjectsRootDirectory, project.Id, packageMarkedForDelete.Package.Id, "_");
                    string deletePath = Path.Combine(context.ProjectsRootDirectory, project.Id, packageMarkedForDelete.Package.Id, "!_");

                    if (Directory.Exists(packageDirectory) && !Directory.Exists(deletePath))
                    {
                        try
                        {
                            if (originalPackageInstance != null)
                                originalPackageInstance.TransferState = PackageTransferStates.Deleting;

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
                        if (originalPackageInstance != null)
                            originalPackageInstance.TransferState = PackageTransferStates.Deleted;
                        packageMarkedForDelete.DownloadProgress.Message = "Deleted";
                    }
                }
            }
        }
    }
}
