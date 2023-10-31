using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace TetrifactClient
{
    /// <summary>
    /// Utility lib of hash functions
    /// </summary>
    public class HashLib
    {

        /// <summary>
        /// Locally utilty function, hex stage of generating hash.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>bytes as hexadecimal string</returns>
        public static string ToHex(byte[] bytes)
        {
            StringBuilder s = new StringBuilder();

            foreach (byte b in bytes)
                s.Append(b.ToString("x2").ToLower());

            return s.ToString();
        }

        /// <summary>
        /// Generates a SHA256 hash of the file at the given path.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>File hash</returns>
        public static string FromFile(string filePath)
        {
            using (FileStream fs = File.OpenRead(filePath))
            using (HashAlgorithm hashAlgorithm = SHA256.Create())
            {
                byte[] hash = hashAlgorithm.ComputeHash(fs);
                return ToHex(hash);
            }
        }

        /// <summary>
        /// Generates a SHA256 hash from a string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FromString(string str)
        {
            Stream stream = StreamsLib.StreamFromString(str);
            using (HashAlgorithm hashAlgorithm = SHA256.Create())
            {
                byte[] hash = hashAlgorithm.ComputeHash(stream);
                return ToHex(hash);
            }
        }

        /// <summary>
        /// Sorts file paths so they are in standard order for hash creation.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public static string[] SortFileArrayForHashing(string[] files)
        {
            Array.Sort(files, (x, y) => String.Compare(x, y));
            return files;
        }
    }
}
