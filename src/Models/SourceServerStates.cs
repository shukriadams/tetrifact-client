namespace TetrifactClient
{
    public enum SourceServerStates
    {
        /// <summary>
        /// default, state has not been determined yet
        /// </summary>
        Unknown,

        /// <summary>
        /// Server is available and operating within expected parameters
        /// </summary>
        Normal,

        /// <summary>
        /// Connection to server is possible, but slow, or experiencing outtages.
        /// </summary>
        Degraded,

        /// <summary>
        /// Server cannot be reached.
        /// </summary>
        Unavailable
    }
}
