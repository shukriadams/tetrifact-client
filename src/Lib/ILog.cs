namespace TetrifactClient
{
    /// <summary>
    /// Simple log implementation
    /// </summary>
    public interface ILog
    {
        void LogError(object error, string description = "");

        void LogInfo(object info, string description = "");

        void LogDebug(object info, string description = "");

        void LogError(string description);

        void LogInfo(string description);

        void LogDebug(string description);

    }
}
