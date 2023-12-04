namespace TetrifactClient
{
    /// <summary>
    /// Simple log implementation
    /// </summary>
    public interface ILog
    {
        void LogError(object error, string description = "");

        /// <summary>
        /// An error happened in some external system like a remote server. Should be reported to user, but not written to text log. 
        /// </summary>
        /// <param name="error"></param>
        /// <param name="description"></param>
        void LogUnstability(object error, string description = "");

        void LogInfo(object info, string description = "");

        void LogDebug(object info, string description = "");

        void LogError(string description);

        void LogInfo(string description);

        void LogDebug(string description);

    }
}
