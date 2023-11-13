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

        public int Attempts { get; set; }

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
                        // if no content length returned, assume server does not support query
                        if (webResponse.Headers.Get("Content-Length") == null) 
                            return 0;

                        return long.Parse(webResponse.Headers.Get("Content-Length"));
                    }
                }
                catch (WebException ex) 
                {
                    Console.WriteLine($"error reading resource size for {url}, attempt {tries}. {ex}");
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
            bool error = false;
            Exception _lastException = null;

            do
            {
                this.Attempts++;

                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    using (Stream stream = base.OpenRead(url))
                    {
                        stream.CopyTo(ms);

                        if (error)
                            _log.LogError(error, $"Succeeded downloading {url} after {this.Attempts} tries.");

                        return ms.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    error = true;
                    _lastException = ex;

                    if (ex is WebException)
                    {
                        WebException wex = ex as WebException;
                        // target not found, abort immediately, no need to retry
                        if (wex.Response is HttpWebResponse && ((HttpWebResponse)wex.Response).StatusCode == HttpStatusCode.NotFound)
                            break;
                    }

                    if (maxTries > 1)
                    {
                        // running in "try many times mode". Log the error, sleep, try again
                        _log.LogError(ex, $"Error downloading {url}, attempt {this.Attempts} of {maxTries}");
                        Thread.Sleep(sleepPerTry);
                    }
                    else
                    {
                        // not retrying, fail immediately
                        throw;
                    }
                }
            }
            while (this.Attempts < maxTries); // put at end to ensure that loops runs at least once for maxtries = 0

            if (_lastException == null)
                throw new Exception($"Too many failed attempts downloading {url}");
            else
                throw new Exception($"Too many failed attempts downloading {url}", _lastException);
        }

        public void DownloadFile(string url, string saveLocation, int maxTries = 1, int sleepPerTry = 1000)
        {
            this.Attempts = 0;
            bool succeeded = false;
            Exception _lastException = null;

            while (this.Attempts < maxTries)
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
                        _lastException = ex;
                        if (_log != null)
                            _log.LogError(ex, $"Error downloading {url}, attempt {this.Attempts} of {maxTries}");

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
                    this.Attempts++;
                }
            }

            if (!succeeded)
                if (_lastException == null)
                    throw new Exception($"Too many failed attempts downloading {url}");
                else
                    throw new Exception($"Too many failed attempts downloading {url}.", _lastException);
        }
    }
}
