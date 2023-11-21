using System;
using System.IO;
using System.Net;
using System.Threading;

namespace TetrifactClient
{
    public class HttpPayloadRequest
    {
        public byte[] Payload { get; private set; }

        /// <summary>
        /// Milliseconds
        /// </summary>
        public double TimeTaken { get; private set; }

        public int Retries { get; private set; }

        public bool Succeeded { get; private set; }

        public string Error { get; private set; }

        public Exception LastException { get; private set; }

        public int MaxTries { get; set; }
 
        public int SleepPerTry { get; set; }

        public string Url { get; private set; }

        public HttpPayloadRequest(string url) 
        {
            this.Url = url;
            this.MaxTries = 1;
        }

        public void Attempt() 
        {
            DateTime start = DateTime.UtcNow;

            try
            {
                Exception _lastException = null;

                while (this.Retries < this.MaxTries)
                {
                    this.Retries++;

                    try
                    {
                        HardenedWebClient client = new HardenedWebClient();
                        client.Timeout = GlobalDataContext.Instance.Timeout;
                        using (MemoryStream ms = new MemoryStream())
                        using (Stream stream = client.OpenRead(this.Url))
                        {
                            stream.CopyTo(ms);
                            this.Payload = ms.ToArray();
                            this.Succeeded = true;
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.LastException = ex;

                        if (ex is WebException)
                        {
                            WebException wex = ex as WebException;
                            // target not found, abort immediately, no need to retry
                            if (wex.Response is HttpWebResponse && ((HttpWebResponse)wex.Response).StatusCode == HttpStatusCode.NotFound) 
                            {
                                this.Error = "Target URL not found.";
                                break;
                            }
                        }

                        if (this.MaxTries > 1)
                            Thread.Sleep(this.SleepPerTry);
                        else
                            // not retrying, fail immediately
                            throw;
                    }
                }

            }
            catch (Exception ex)
            {
                this.LastException = ex;
            }
            finally 
            {
                TimeSpan taken = DateTime.UtcNow - start;
                this.TimeTaken = taken.TotalMilliseconds;
            }
        }

    }
}
