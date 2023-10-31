using System.Collections.Generic;

namespace TetrifactClient
{
    public static class PackageDownloadProgressStore
    {
        private static Dictionary<string, PackageDownloadProgress> _progress = new Dictionary<string, PackageDownloadProgress> ();

        public static PackageDownloadProgress Get(string serverAddress, string packageId) 
        {
            string key = $"{HashLib.FromString(serverAddress)}__{packageId}";
            
            lock (_progress) 
            {
                if (_progress.ContainsKey(key))
                    return _progress[key];

                PackageDownloadProgress progress = new PackageDownloadProgress ();
                progress.ServerAddress = serverAddress;
                progress.PackageId = packageId;
                _progress.Add(key, progress);

                return progress;
            }
        }
    }
}
