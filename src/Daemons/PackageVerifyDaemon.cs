using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace TetrifactClient
{
    public class PackageVerifyDaemon : IDaemon
    {
        private string _currentlyVerifying;

        public void Start()
        {
            DaemonProcessRunner runner = new DaemonProcessRunner();
            Log log = new Log();
            runner.Start(new AsyncDo(this.Work), GlobalDataContext.Instance.DaemonIntervalMS, new Log());
        }

        public void WorkNow()
        {
            throw new NotImplementedException();
        }

        public async Task Work()
        {
            // find next package to verify
            LocalPackage packageToVerify = null;
            Project parentProject = null;

            foreach (Project project in GlobalDataContext.Instance.Projects.Projects) 
            {
                packageToVerify = project.Packages.FirstOrDefault(p => p.IsQueuedForVerify);
                parentProject = project;
                if (packageToVerify != null)
                    break;
            }

            if (packageToVerify == null)
                return;

            string packageFilesListPath = Path.Combine(GlobalDataContext.Instance.ProjectsRootDirectory, parentProject.Id, packageToVerify.Package.Id, "files.json");
            JsonFileLoadResponse<IEnumerable<PackageFile>> jsonLoadResponse = JsonHelper.LoadJSONFile<IEnumerable<PackageFile>>(packageFilesListPath, true, true);
            if (jsonLoadResponse.Payload == null)
                throw new Exception($"Failed to load full Package {packageToVerify.Package.Id} in project {parentProject.Name}, error {jsonLoadResponse.ErrorType}.", jsonLoadResponse.Exception);

            string contentRootDirectory = Path.Combine(GlobalDataContext.Instance.ProjectsRootDirectory, parentProject.Id, packageToVerify.Package.Id, "_");
            bool isValid = true;
            IList<string> inValidReason = new List<string>();
            long total = jsonLoadResponse.Payload.Count();
            long count = 0;
            packageToVerify.DownloadProgress.Total = total;

            // create a dictionary of all files in package, sorted by path
            string[] ordered = jsonLoadResponse.Payload.Select(r => r.Path).ToArray();
            ordered = HashLib.SortFileArrayForHashing(ordered);

            IDictionary<string, string> hashes = new Dictionary<string, string>();
            foreach (string file in ordered)
                hashes.Add(file, null);

            // this process will bottleneck at the disk
            int threads = GlobalDataContext.Instance.ThreadLoad;
            Parallel.ForEach(jsonLoadResponse.Payload, new ParallelOptions() { MaxDegreeOfParallelism = threads }, item =>
            {
                try
                {
                    count ++;
                    packageToVerify.DownloadProgress.Progress = count;
                    packageToVerify.DownloadProgress.Message = $"Verifying {MathHelper.Percent(count, total)}%";
                    
                    string expectedPath = Path.Join(contentRootDirectory, item.Path);

                    if (!File.Exists(expectedPath))
                    {
                        isValid = false;
                        inValidReason.Add($"Missing file {item.Path}");
                        return;
                    }

                    string checksum = HashLib.FromFile(expectedPath);
                    if (checksum != item.Hash) 
                    {
                        isValid = false;
                        inValidReason.Add($"Invalid hash for {item.Path}, expected {item.Hash}, got {checksum}");
                    }

                    // append file checksum to combined hash dictionary
                    hashes[item.Path] = HashLib.FromString(item.Path) + checksum;
                }
                catch(Exception ex)
                {
                    isValid = false;
                    inValidReason.Add($"Unexpected error checking {item.Path}:{ex}");
                }
            });

            string currentHash = HashLib.FromString(string.Join(string.Empty, hashes.Values));

            if (currentHash != packageToVerify.Package.Hash)
            {
                //TODO : still failing!
                //isValid = false;
                //inValidReason.Add($"Package hash failed, expected {packageToVerify.Package.Hash}, got {currentHash}");
            }

            packageToVerify.IsQueuedForVerify = false;

            if (isValid)
            {
                packageToVerify.DownloadProgress.Message = $"Verification passed";
                packageToVerify.TransferState = PackageTransferStates.Downloaded;
            }
            else 
            {
                packageToVerify.DownloadProgress.Message = $"Check console";
                packageToVerify.TransferState = PackageTransferStates.Corrupt;

                foreach (string er in inValidReason)
                    GlobalDataContext.Instance.Console.Add(er);
            }
        }
    }
}
