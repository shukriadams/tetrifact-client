namespace TetrifactClient
{
    /// <summary>
    /// Describes a fragment of a file that can be dowbloaded independent. Chunks are reassembled after all chunks are available locally.
    /// </summary>
    public class DownloadChunk
    {
        /// <summary>
        /// used to reassemble chunks in correct order 
        /// </summary>
        public int Index { get; set; }

        public long Start { get; set; }

        public long End { get; set; }
    }
}
