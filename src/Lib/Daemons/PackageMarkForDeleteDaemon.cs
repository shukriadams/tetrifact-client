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
                foreach (LocalPackage package in project.Packages.Where(package => package.IsMarkedForDeleteOrBeingDeleting()))
                {
                    IEnumerable<string> subDirs = Directory.GetDirectories(PathHelper.GetPackageDirectoryPath(context, project, package));
                    IEnumerable<string> unmarkedSubDirs = subDirs.Where(dir => !Path.GetFileName(dir).StartsWith("!"));

                    if (unmarkedSubDirs.Any())
                    {
                        package.TransferState = PackageTransferStates.Deleting;

                        foreach (string unmarkedSubDir in unmarkedSubDirs)
                        {
                            string packageContentPathDelete = Path.Join(Path.GetDirectoryName(unmarkedSubDir), $"!{Path.GetFileName(unmarkedSubDir)}");

                            try
                            {
                                // if content directory is not marked, try marking
                                if (Directory.Exists(packageContentPathDelete))
                                    continue;

                                Directory.Move(unmarkedSubDir, packageContentPathDelete);
                            }
                            catch (Exception)
                            {
                                // ignore errors here, they will be caused by access locks
                                continue;
                            }
                        }
                    }
                    
                    if (!subDirs.Any())
                    {
                        package.TransferState = PackageTransferStates.Deleted;
                    }
                }
            }
        }
    }
}
