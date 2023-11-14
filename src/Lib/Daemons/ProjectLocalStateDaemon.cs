using Avalonia.Threading;
using DynamicData;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TetrifactClient
{
    public class ProjectLocalStateDaemon : IDaemon
    {
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
            Dispatcher.UIThread.Post(() => {

                foreach (Project project in GlobalDataContext.Instance.Projects.Projects)
                    project.PopulatePackageList();

                GlobalDataContext.Instance.Console.Add($"TEST {DateTime.Now}");
            },
            DispatcherPriority.Background);


            return;
            // Start the job and return immediately
            Dispatcher.UIThread.InvokeAsync(() => {
                foreach (Project project in GlobalDataContext.Instance.Projects.Projects)
                {

                    string localProjectPackagesDirectory = Path.Combine(GlobalDataContext.Instance.GetProjectsDirectoryPath(), project.Id, "packages");
                    if (!Directory.Exists(localProjectPackagesDirectory))
                        return;

                    IEnumerable<string> packageIds = Directory.
                        GetDirectories(localProjectPackagesDirectory).
                        Select(p => Path.GetFileName(p));

                    List<LocalPackage> newPackages = new List<LocalPackage>();
                    foreach (string packageId in packageIds)
                    {
                        if (project.Packages.Items.Any(p => p.Package.Id == packageId))
                            continue;

                        string packageRawJson = string.Empty;
                        string packageCorePath = Path.Combine(localProjectPackagesDirectory, packageId, "remote.json");
                        if (!File.Exists(packageCorePath))
                            continue;

                        JsonFileLoadResponse<LocalPackage> baseLoadReponse = JsonHelper.LoadJSONFile<LocalPackage>(packageCorePath, true, true);

                        // Todo : handle error bettter
                        if (baseLoadReponse.ErrorType != JsonFileLoadResponseErrorTypes.None)
                            throw new Exception($"failed to load {packageCorePath}, {baseLoadReponse.ErrorType} {baseLoadReponse.Exception}");

                        baseLoadReponse.Payload.DiskPath = packageCorePath;
                        newPackages.Add(baseLoadReponse.Payload);
                    }

                    // sort and apply filters
                    string requiredTags = string.IsNullOrEmpty(project.RequiredTags) ? string.Empty : project.RequiredTags;
                    string ignoreTags = string.IsNullOrEmpty(project.IgnoreTags) ? string.Empty : project.RequiredTags;
                    string[] requiredTagsArray = requiredTags.Split(",", StringSplitOptions.RemoveEmptyEntries);
                    string[] ignoreTagsArray = ignoreTags.Split(",", StringSplitOptions.RemoveEmptyEntries);

                    IEnumerable<LocalPackage> tempPackages = newPackages.OrderByDescending(p => p.Package.CreatedUtc);
                    if (requiredTags.Any())
                        tempPackages = from package in tempPackages
                                       where !package.Package.Tags.Except(requiredTagsArray).Any()
                                       select package;

                    if (ignoreTagsArray.Any())
                        tempPackages = from package in tempPackages
                                       where package.Package.Tags.Except(ignoreTagsArray).Any()
                                       select package;

                    if (!tempPackages.Any())
                        return;

                    project.Packages.Items.AddRange(tempPackages);
                    //project.Packages.add

                    foreach (var package in project.Packages.Items)
                        package.EnableAutoSave();

                    //tempPackages = tempPackages.OrderByDescending(p => p.Package.CreatedUtc);
                    //project.Packages = new ObservableCollection<LocalPackage> (tempPackages);
                    System.Diagnostics.Debug.WriteLine($"PopulatePackageList:{DateTime.Now.Second}:{project.Packages.Items.Count}");
                }

            },
            DispatcherPriority.Background);

        }
    }
}

