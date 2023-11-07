using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;

namespace TetrifactClient
{
    public class JsonHelper
    {
        /// <summary>
        /// Downloads and parses JSON of type T from given url
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T DownloadJson<T>(string url)
        {
            try
            {
                HardenedWebClient webClient = new HardenedWebClient();
                string rawResponse = webClient.DownloadString(url);

                dynamic response = JsonConvert.DeserializeObject(rawResponse);
                if (response == null)
                    throw new Exception($"failed to get json from {url}");

                return JsonConvert.DeserializeObject<T>(response.success != null ? response.success.ToString() : response.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download json at url {url}", ex);
            }
        }

        /// <summary>
        /// Downloads and parses manifest package json from tetrifact
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="packageId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Package DownloadManifest(string serverAddress, string packageId)
        {
            try
            {
                // note that manifests are always fetched from primary build server
                string manifestUrl = $"{serverAddress}/v1/packages/{packageId}";
                HardenedWebClient webClient = new HardenedWebClient();
                string manifestRaw = webClient.DownloadString(manifestUrl);

                // get size of package
                dynamic response = JsonConvert.DeserializeObject(manifestRaw);
                if (response == null || response.success == null)
                    throw new Exception($"Received error response trying to get manifest for package {packageId}");

                return JsonConvert.DeserializeObject<Package>(response.success.package.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download manifest for package {packageId}", ex);
            }
        }

        /// <summary>
        /// Downloads and parses package diff json from tetrifact.
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="upstreamPackage"></param>
        /// <param name="downstreamPackage"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static PackageDiffResponse DownloadDiff(string serverAddress, string upstreamPackage, string downstreamPackage)
        {
            try
            {
                HardenedWebClient webClient = new HardenedWebClient();
                string diffRaw = webClient.DownloadString($"{serverAddress}/v1/packages/diff/{upstreamPackage}/{downstreamPackage}");
                dynamic responseDiff = JsonConvert.DeserializeObject(diffRaw);
                if (responseDiff == null || responseDiff.success == null)
                    return new PackageDiffResponse {
                        ResponseType = PackageDiffResponseTypes.Unexpected,
                        Message = $"Received error response trying to get diff for packages {upstreamPackage} and {downstreamPackage}" 
                    };

                return new PackageDiffResponse 
                {
                    PackageDiff = JsonConvert.DeserializeObject<PackageDiff>(responseDiff.success.packagesDiff.ToString())
                };
            }
            catch (WebException ex)
            {
                HttpWebResponse response = ex.Response as HttpWebResponse;
                if (response != null && response.StatusCode == HttpStatusCode.NotFound)
                    using (StreamReader r = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        string content = r.ReadToEnd();
                        if (content.Contains(upstreamPackage))
                            return new PackageDiffResponse { ResponseType = PackageDiffResponseTypes.UpstreamPackageNotFound };
                    }

                return new PackageDiffResponse
                {
                    Message = $"Failed to download diff between packages {upstreamPackage} and {downstreamPackage}",
                    Exception = ex
                };
            }
            catch (Exception ex)
            {
                return new PackageDiffResponse
                {
                    Message = $"Failed to download diff between packages {upstreamPackage} and {downstreamPackage}",
                    Exception = ex
                };
            }
        }

        /// <summary>
        /// Loads Json from local file system
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static JsonFileLoadResponse<T> LoadJSONFile<T>(string path, bool failIfFileNotFound, bool failIfNull)
        {
            if (!File.Exists(path) && failIfFileNotFound)
                return new JsonFileLoadResponse<T>(JsonFileLoadResponseErrorTypes.FileNotFound);

            string rawJson;
            try
            {
                rawJson = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                return new JsonFileLoadResponse<T>(JsonFileLoadResponseErrorTypes.FileLoadError, ex);
            }

            T payload = default(T);
            try 
            {
                payload  = JsonConvert.DeserializeObject<T>(rawJson);
            } 
            catch (Exception ex) 
            {
                return new JsonFileLoadResponse<T>(JsonFileLoadResponseErrorTypes.JsonParseError, ex);
            }

            if (payload == null && failIfNull)
                return new JsonFileLoadResponse<T>(JsonFileLoadResponseErrorTypes.NullParseError);

            return new JsonFileLoadResponse<T>(payload);
        }
    }
}
