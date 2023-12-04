using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity;

namespace TetrifactClient
{
    /// <summary>
    /// Downloads a file as a series of chunks, which are then re-assembled. This minimizes retry risk. Chunks must download successfully before being
    /// written to disk, and persist after download, so this process can be restarted and resume with existing chunks.
    /// </summary>
    public class ChunkedDownloader : ICancel
    {
        #region FIELDS

        private int _currentChunk;

        private int _totalChunkCount;

        public event OnProgress OnChunkDownloaded;

        public event OnProgress OnChunkAssembled;

        public event OnPersistantProcessError OnError;

        public int RetryAttempts { get; set; }

        private ILog _log;

        #endregion

        #region PROPERTIES

        public IsTrueLookup CancelCheck { get; set; }

        public bool Succeeded { get; set; }

        #endregion

        #region CTORS

        public ChunkedDownloader()
        {
            _log = App.UnityContainer.Resolve<ILog>();
        }

        #endregion

        #region METHODS

        private void HandleChunkDownloadEvent()
        {
            decimal p = (decimal)_currentChunk / (decimal)_totalChunkCount;
            int percent = (int)Math.Round((decimal)(p * 100), 0);
            OnChunkDownloaded?.Invoke(string.Empty, _currentChunk, _totalChunkCount, percent);
            _currentChunk++;
        }

        private void HandleChunkAssembledEvent(int current, int total)
        {
            decimal p = (decimal)current / (decimal)total;
            int percent = (int)Math.Round((decimal)(p * 100), 0);
            OnChunkAssembled?.Invoke(string.Empty, current, total, percent);
            _currentChunk++;
        }

        private void HandleErrorEvent(Exception ex)
        {
            OnError?.Invoke(ex);
        }

        public void Download(string url, string destinationFilePath, long chunkSize, int numberOfParallelDownloads = 0)
        {
            if (destinationFilePath.Length > Constants.MAX_PATH_LENGTH)
                throw new Exception($"{destinationFilePath} exceeds max safe path length");

            HardenedWebClient client = new HardenedWebClient();
            client.Timeout = GlobalDataContext.Instance.Timeout;
            long totalStreamLength = 0;

            // try to get resource in chunks, this requires we get size of resource on server.
            try
            {
                totalStreamLength = client.GetResourceSize(url, 10);
            }
            catch (TimeoutException ex)
            {
                throw new TimeoutException ($"Chunked download could not get resource size for {url} in timely fashion.", ex);
            }

            // force delete local file if it already exists, this process uses progressive downloads and can be run again over an existing failed attempt
            if (File.Exists(destinationFilePath))
                File.Delete(destinationFilePath);

            FileSystemHelper.CreateDirectory(Path.GetDirectoryName(destinationFilePath));

            // break download up into projected chunk blocks, these need an index nr so we can re-assembly chunks in the correct order
            ConcurrentDictionary<int, string> chunkFiles = new ConcurrentDictionary<int, string>();
            List<DownloadChunk> projectedChunks = new List<DownloadChunk>();
            long streamPosition = 0;
            int chunkCount = 0;

            // generate a list of objects with chunk start+end positions, along with with index to order them. We use this to request blocks of 
            // data from the server. HTTP streaming wants a start and end position to stream from.
            while (streamPosition < totalStreamLength)
            {
                // projected chunk size should be the requested size, unless we are at end of stream, then chunk size should be whatever is left in stream
                long sizeOfThisChunk = chunkSize;
                if (streamPosition + chunkSize > totalStreamLength)
                    sizeOfThisChunk = totalStreamLength - streamPosition;

                projectedChunks.Add(new DownloadChunk
                {
                    Index = chunkCount,
                    Start = streamPosition,
                    End = streamPosition + sizeOfThisChunk
                });

                // needs +1 to start at next chunk point, else next chunk contains last character of previous chunk
                streamPosition += sizeOfThisChunk + 1; 

                chunkCount ++;
            }

            _totalChunkCount = projectedChunks.Count;

            // download to a temp direct inside of system temp directory, for this app + download url. This gives us a fixed location
            // that we can resume at, but within reason. Directory will be OS managed and cleaned up as necessary.
            string tempDirectory = Path.Combine(Path.GetTempPath(), System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, HashLib.FromString(url));
            Directory.CreateDirectory(tempDirectory);

            // download chunks to temp files in $USER/Appdata/local/tmp
            Parallel.ForEach(projectedChunks, new ParallelOptions() { MaxDegreeOfParallelism = numberOfParallelDownloads }, range =>
            {
                try
                {
                    // break out of look if cancel set from parent process
                    if (this.CancelCheck != null && this.CancelCheck())
                        return;

                    HardenedWebClient webClient = new HardenedWebClient();
                    webClient.Timeout = GlobalDataContext.Instance.Timeout;
                    webClient.Range = new Range { Start = range.Start, End = range.End };

                    byte[] data = webClient.DownloadBytes(url, this.RetryAttempts); 
                    string tempFilePath1 = Path.Combine(tempDirectory, $"{Guid.NewGuid()}.chunk");
                    string tempFilePath2 = Path.Combine(tempDirectory, $"{range.Index}.chunk");

                    if (!File.Exists(tempFilePath2)) 
                    {
                        if (File.Exists(tempFilePath1))
                            File.Delete(tempFilePath1);

                        using (FileStream fileStream = new FileStream(tempFilePath1, FileMode.Create, FileAccess.Write, FileShare.Write))
                            fileStream.Write(data, 0, data.Length);

                        chunkFiles.TryAdd(range.Index, tempFilePath2);
                        File.Move(tempFilePath1, tempFilePath2);
                    }

                    HandleChunkDownloadEvent();
                }
                catch (Exception ex)
                {
                    HandleErrorEvent(ex);
                }
            });

            FileSystemHelper.CreateDirectory(Path.GetDirectoryName(destinationFilePath));

            // assemble chunks into final file
            using (FileStream destinationStream = new FileStream(destinationFilePath, FileMode.Append))
            {
                int count = 0;

                foreach (var tempFile in chunkFiles.OrderBy(b => b.Key))
                {
                    // break out of look if cancel set from parent process
                    if (this.CancelCheck != null && this.CancelCheck())
                        continue;

                    byte[] tempFileBytes = File.ReadAllBytes(tempFile.Value);
                    destinationStream.Write(tempFileBytes, 0, tempFileBytes.Length);
                    File.Delete(tempFile.Value);
                    count++;
                    HandleChunkAssembledEvent(count, chunkFiles.Count);
                }
            }

            this.Succeeded = true;
        }

        #endregion
    }
}
