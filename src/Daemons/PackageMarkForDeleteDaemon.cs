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
                foreach (LocalPackage packageMarkedForDelete in project.Packages.Where(package => package.IsMarkedForDeleteOrBeingDeleting()))
                {
                    IEnumerable<string> subDirs = Directory.GetDirectories(PathHelper.GetPackageDirectoryPath(context, project, packageMarkedForDelete));
                    IEnumerable<string> unmarkedSubDirs = subDirs.Where(dir => !Path.GetFileName(dir).StartsWith("!"));

                    if (unmarkedSubDirs.Any())
                    {
                        packageMarkedForDelete.TransferState = PackageTransferStates.Deleting;

                        foreach (string unmarkedSubDir in unmarkedSubDirs)
                        {
                            // move delete content to guid incase previous delete attempt failed
                            // add time to pathname as a way to track potentially slow delete issues
                            // todo : warn if another ! marked dir already exists here
                            string packageContentPathDelete = Path.Join(Path.GetDirectoryName(unmarkedSubDir), $"!{Guid.NewGuid()}-{DateTime.UtcNow.ToFSString()}");

                            try
                            {
                                Directory.Move(unmarkedSubDir, packageContentPathDelete);
                            }
                            catch (Exception)
                            {
                                // ignore errors here, they will be caused by access locks
                                continue;
                            }
                        }
                    }

                    // package is marked for delete only when all its sub dirs are deleted. The actual deleting is done by another
                    // daemon, the PackageDeleteDaemon, which deletes directories marked for delete by this daemon.
                    if (!subDirs.Any()) 
                    {
                        packageMarkedForDelete.TransferState = PackageTransferStates.Deleted;
                        packageMarkedForDelete.DownloadProgress.Message = "Deleted";
                    }
                }
            }
        }
    }
}
