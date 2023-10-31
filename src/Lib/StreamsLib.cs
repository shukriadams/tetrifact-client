using System.IO;
using System.Text;

namespace TetrifactClient
{
    public class StreamsLib
    {
        /// <summary>
        /// Converts a string to stream.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static MemoryStream StreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? string.Empty));
        }
    }
}
