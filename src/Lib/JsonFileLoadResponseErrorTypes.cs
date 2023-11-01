
namespace TetrifactClient
{
    /// <summary>
    /// Error types returned by direct file JSON loading
    /// </summary>
    public enum JsonFileLoadResponseErrorTypes
    {
        /// <summary>
        /// // 
        /// </summary>
        None = 0,

        /// <summary>
        /// File was not found
        /// </summary>
        FileNotFound = 1,

        /// <summary>
        /// File was found but threw an exception on load, check exception object
        /// </summary>
        FileLoadError = 2,

        /// <summary>
        /// Json parse threw an exception, see exception object
        /// </summary>
        JsonParseError = 2,

        /// <summary>
        /// Json parsed but returned null where a null was not expected.
        /// </summary>
        NullParseError = 3,

        /// <summary>
        /// Something else happened, see exception object
        /// </summary>
        UnhandledError = 4
    }
}
