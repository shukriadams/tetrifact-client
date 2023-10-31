using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity;

namespace TetrifactClient.Lib.Daemons
{
    /// <summary>
    /// Automatically queues packages to be downloaded. Does not
    /// </summary>
    public class PackageDownloadAutoQueueDaemon
    {
        private ILog _log;

        private bool _busy;

        private Preferences _preferences;

        private GlobalDataContext _globalContext;

        public PackageDownloadAutoQueueDaemon(ILog log, Preferences preferences, GlobalDataContext globalContext)
        {
            _preferences = preferences;
            _log = log;
            _globalContext = globalContext;
        }

        public void Start()
        {
            DaemonProcessRunner runner = new DaemonProcessRunner();
            Log log = new Log();
            runner.Start(new AsyncDo(this.Work), GlobalDataContext.Instance.DaemonIntervalMS, new Log());
        }

        private async Task Work()
        {
            foreach (Project project in GlobalDataContext.Instance.Projects.Projects)
            {
                try
                {
                    // no need to do anything if auto disabled
                    if (!project.AutoDownload)
                        continue;

                    HardenedWebClient webClient = new HardenedWebClient();

                    
                    packages = packages.Where(r => r.Tags.Contains(Settings.Instance.AutoDownloadTag));

                    // stream filter
                    if (!string.IsNullOrEmpty(preferences.StreamFilter))
                        packages = packages.Where(r => r.Tags.Contains($"stream:{preferences.StreamFilter}"));

                    packages = packages.OrderByDescending(r => r.CreatedUtc);

                    // remove all ignored packages
                    IEnumerable<Package> latestPackages = packages.Where(r => !downloadFlags.Contains($"ignore_{r.Id}")).OrderByDescending(r => r.CreatedUtc).Take(preferences.BuildDownloadCount);

                    // clean up existing by first renaming folder so we don't partially delete a build that's in use
                    string[] existingDownloads = Directory.GetDirectories(preferences.DownloadsFolder);
                    foreach (string existingDownloadFolder in existingDownloads)
                    {
                        string directoryBasename = Path.GetFileName(existingDownloadFolder);

                        // if the existing build is in the list to download, ignore it
                        if (latestPackages.Where(r => r.Id == directoryBasename).Any())
                            continue;

                        // if folder does not contain package .exe, ignore it. this is a safety mechanism to prevent deleting unintended folders
                        if (!File.Exists(Path.Combine(existingDownloadFolder, Settings.Instance.PackageExecutable)))
                            continue;

                        // if build is pinned, ignore it
                        if (File.Exists(Path.Combine(preferences.DownloadsFolder, $"pin_{directoryBasename}")))
                            continue;

                        // if directory is being unpacked, ignore it 
                        if (directoryBasename.StartsWith("~"))
                            continue;

                        // if build is already marked for delete, try to delete it
                        if (directoryBasename.StartsWith("!DELETE!_"))
                        {
                            try
                            {
                                Directory.Delete(existingDownloadFolder, true);
                            }
                            catch (IOException)
                            {
                                // ignore
                                Console.Write($"IOException trying to delete {existingDownloadFolder}, content should not be locked, but will retry later.");
                            }
                            catch (Exception ex)
                            {
                                _log.LogError(ex);
                            }

                            continue;
                        }

                        // if the total number of playable builds is less than required, wait on deleting.
                        if (localPackages.Builds.Where(b => b.DownloadState.Value == DownloadStates.Downloaded).Count() <= preferences.BuildDownloadCount)
                            continue;

                        // user wants to keep all builds forever, no need to proceed to delete 
                        if (preferences.ForceKeepBuilds)
                            continue;

                        // if reach here directory has no reason to exist, mark it for delete by renaming it to start with "!DELETE!_" flag. We rename a dir before
                        // deleting to easily and cleanly fail if the package exe in the directory is currently being played - the rename will fail, but will be tried again
                        // later and will eventually succeed.
                        try
                        {
                            Directory.Move(existingDownloadFolder, Path.Combine(Path.GetDirectoryName(existingDownloadFolder), $"!DELETE!_{directoryBasename}"));
                        }
                        catch (IOException)
                        {
                            // ignore these, download is most likely being used
                            Console.Write($"IOException trying to delete {existingDownloadFolder}, assuming content in use, retrying later.");
                        }
                        catch (Exception ex)
                        {
                            _log.LogError(ex);
                        }

                    } // foreach

                    foreach (Package package in latestPackages)
                    {
                        this.QueueBuild(package.Id, false);
                    }

                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Unexpected error getting package list from build server");
                }
                finally
                {
                    _busy = false;
                    await Task.Delay(Settings.Instance.BuildPollInterval);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageId"></param>
        public void QueueBuild(string packageId, bool forcePin)
        {
            try
            {
                Preferences preferences = _preferencesManager.Load();

                // build has already been downloaded
                if (Directory.Exists(Path.Combine(preferences.DownloadsFolder, packageId)))
                    return;

                if (File.Exists(Path.Combine(preferences.DownloadsFolder, packageId)))
                    return;

                // if build is ignored, ignore it
                if (File.Exists(Path.Combine(preferences.DownloadsFolder, $"ignore_{packageId}")))
                    return;

                File.WriteAllText(Path.Combine(preferences.DownloadsFolder, $"dl_{packageId}"), string.Empty);
                if (forcePin)
                    File.WriteAllText(Path.Combine(preferences.DownloadsFolder, $"pin_{packageId}"), string.Empty);
            }
            catch (Exception ex)
            {
                _log.LogError(ex);
            }
        }

    }
}
