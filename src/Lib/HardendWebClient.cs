using System;
using System.IO;
using System.Net;
using System.Threading;
using Unity;

namespace TetrifactClient
{
    /// <summary>
    /// Extends webclient, increases stupid defaualt timeout of 30 seconds, adds built-in retry
    /// </summary>
    public class HardenedWebClient : WebClient
    {
        public Range Range { get; set; }

        private ILog _log;

        public HardenedWebClient()
        {
            Proxy = WebRequest.DefaultWebProxy;
            _log = App.UnityContainer.Resolve<ILog>();
        }

        /// <summary>
        /// Adds support for ranged http requests
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest request = base.GetWebRequest(uri);
            request.Timeout = 60 * 60 * 1000;

            if (this.Range != null)
            {
                HttpWebRequest httpRequest = (HttpWebRequest)request;
                httpRequest.AddRange(this.Range.Start, this.Range.End);
            }

            return request;
        }

        public long GetResourceSize(string url, int maxTries = 1, int sleepPerTry = 1000)
        {
            int tries = 0;

            // get target file size
            WebRequest webRequest = HttpWebRequest.Create(url);
            webRequest.Timeout = 999999; // archive generation can take a long time

            while (tries < maxTries)
            {
                tries++;
                try
                {
                    using (WebResponse webResponse = webRequest.GetResponse())
                    {
                        return long.Parse(webResponse.Headers.Get("Content-Length"));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"error reading resource size for {url}, attempt {tries}");
                    Thread.Sleep(sleepPerTry);
                    sleepPerTry = sleepPerTry * 2; // double wait each timeout
                }
            }

            throw new TimeoutException($"Failed to get resource size for {url} after {tries} tries");
        }

        /// <summary>
        /// Download bytes, with retries.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="maxTries"></param>
        /// <param name="sleepPerTry">Millisecs to pause between failed attempts</param>
        /// <returns></returns>
        public byte[] DownloadBytes(string url, int maxTries = 1, int sleepPerTry = 1000)
        {
            int attempt = 0;
            bool error = false;

            while (attempt < maxTries)
            {
                attempt++;

                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    using (Stream stream = base.OpenRead(url))
                    {
                        stream.CopyTo(ms);

                        if (error)
                            _log.LogError(error, $"Succeeded downloading {url} after {attempt} tries.");

                        return ms.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    error = true;

                    if (maxTries > 1)
                    {
                        // we're running this class in "try many times mode". Log the error, sleep, try again
                        _log.LogError(ex, $"Error downloading {url}, attempt {attempt} of {maxTries}");

                        Thread.Sleep(sleepPerTry); // wait a bit for a random act of god to magically heal the network 
                    }
                    else
                    {
                        // we're not retrying, rethrow exception and fail immediately
                        throw ex;
                    }
                }
            }

            throw new Exception($"Too many failed attempts downloading {url}");
        }

        public void DownloadFile(string url, string saveLocation, int maxTries = 1, int sleepPerTry = 1000)
        {
            int attempt = 0;
            bool succeeded = false;

            while (attempt < maxTries)
            {
                try
                {
                    if (saveLocation.Length > Constants.MAX_PATH_LENGTH)
                        throw new Exception($"{saveLocation} exceeds max safe path length");

                    FileSystemHelper.CreateDirectory(Path.GetDirectoryName(saveLocation));
                    base.DownloadFile(url, saveLocation);
                    succeeded = true;
                    break;
                }
                catch (Exception ex)
                {
                    if (maxTries > 1)
                    {
                        if (_log != null)
                            _log.LogError(ex, $"Error downloading {url}, attempt {attempt} of {maxTries}");

                        Thread.Sleep(sleepPerTry); // wait a bit for a random act of god to magically heal the network 
                    }
                    else
                    {
                        // if we're not retrying, throw exception like vanilla base method would
                        throw ex;
                    }
                }
                finally
                {
                    attempt++;
                }
            }

            if (!succeeded)
                throw new Exception($"Too many failed attempts downloading {url}");
        }
    }
}
