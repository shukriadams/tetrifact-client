using System;
using System.IO;
using System.Security.Cryptography;

namespace TetrifactClient
{
    public class FileDecryptor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <param name="keysize"></param>
        /// <param name="fileInPath"></param>
        /// <param name="fileOutPath"></param>
        public static void DecryptAESEncoded(string key, string iv, int keysize, string fileInPath, string fileOutPath)
        {
            Aes aes = Aes.Create();
            aes.KeySize = keysize;
            aes.Key = Convert.FromBase64String(key);
            byte[] IV = Convert.FromBase64String(iv);

            using (FileStream inputStream = new FileStream(fileInPath, FileMode.Open))
            {
                ICryptoTransform transform = aes.CreateDecryptor(Convert.FromBase64String(key), IV);

                using (FileStream outputStream = new FileStream(fileOutPath, FileMode.Create))
                {
                    int count = 0;
                    int offset = 0;
                    int blockSizeBytes = 1024;// aes.BlockSize / 8;
                    byte[] data = new byte[blockSizeBytes];

                    using (CryptoStream cryptoStream = new CryptoStream(outputStream, transform, CryptoStreamMode.Write))
                    {
                        do
                        {
                            count = inputStream.Read(data, 0, blockSizeBytes);
                            offset += count;
                            cryptoStream.Write(data, 0, count);

                        } while (count > 0);

                        cryptoStream.FlushFinalBlock();
                    }
                }
            }
        }
    }

}
