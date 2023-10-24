using System;
using System.IO;
using System.Net;
using System.Threading;

namespace TetrifactClient
{
    public class HttpLocalFileRequest
    {
        #region PROPERTIES

        public string SavePath { get; private set; }

        public double TimeTaken { get; private set; }

        public int Retries { get; private set; }

        public bool Succeeded { get; private set; }

        public string Error { get; private set; }

        public int SleepPerTry { get; set; }

        public string Url { get; private set; }

        public int MaxTries { get; set; }

        public Exception LastException { get; private set; }

        #endregion

        #region CTORS

        public HttpLocalFileRequest(Uri uri, string savePath)
        {
            this.Url = uri.ToString();
            this.MaxTries = 1;
            this.SavePath = savePath;
        }

        public HttpLocalFileRequest(string url, string savePath) 
        {
            this.Url = url;
            this.MaxTries = 1;
            this.SavePath = savePath;
        }

        #endregion

        #region METHODS

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
                        WebClient client = new WebClient();
                        byte[] data = client.DownloadData(this.Url);

                        using (FileStream fileStream = new FileStream(this.SavePath, FileMode.Create, FileAccess.Write, FileShare.Write))
                        {
                            fileStream.Write(data, 0, data.Length);
                        }
                        this.Succeeded = true;
                        return;
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

        #endregion
    }
}
