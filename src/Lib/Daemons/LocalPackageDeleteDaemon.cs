using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TetrifactClient
{
    /// <summary>
    /// Deletes packages that have been marked for delete, as well as hanging downloads
    /// </summary>
    public class LocalPackageDeleteDaemon : IDaemon
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
                foreach (LocalPackage package in project.Packages.Where(package => package.IsMarkedForDelete()))
                {
                    package.TransferState = PackageTransferStates.Deleting;
                    string packageContentPath = PathHelper.GetPackageDirectoryPath(context, project, package);
                    string packageContentPathDelete = Path.Join(PathHelper.GetPackageDirectoryRootPath(context, project, package), "!content");

                    try
                    {
                        // if content directory is not marked, try marking
                        if (Directory.Exists(packageContentPath))
                            Directory.Move(packageContentPath, packageContentPathDelete);
                    }
                    catch (Exception ex)
                    {
                        // ignore errors here, they will be caused by access locks
                        continue;
                    }

                    try
                    {
                        // delete stuff
                        if (Directory.Exists(packageContentPathDelete))
                            Directory.Delete(packageContentPathDelete, true);

                        package.TransferState = PackageTransferStates.Deleted;
                    }
                    catch (Exception ex)
                    {
                        package.TransferState = PackageTransferStates.DeleteFailed;
                        _log.LogError(ex);
                    }
                }
            }
        }
    }
}
