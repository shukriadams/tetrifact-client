using System.Collections.Generic;

namespace TetrifactClient
{
    /// <summary>
    /// REMOVE
    /// </summary>
    public static class PackageTransferProgressStore
    {
        private static Dictionary<string, PackageTransferProgress> _progress = new Dictionary<string, PackageTransferProgress> ();

        public static PackageTransferProgress Get(Project project, LocalPackage package) 
        {
            string key = $"{HashLib.FromString(project.Id)}__{package.Package.Id}";
            
            lock (_progress) 
            {
                if (_progress.ContainsKey(key))
                    return _progress[key];

                PackageTransferProgress progress = new PackageTransferProgress ();
                //progress.Project = project;
                //progress.PackageId = package.Package.Id;

                _progress.Add(key, progress);

                return progress;
            }
        }
    }
}
