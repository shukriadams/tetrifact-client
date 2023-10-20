using System;
using System.IO;
using System.Threading;
using Unity;

namespace TetrifactClient
{
    public static class FileSystemHelper
    {
        /// <summary>
        /// Creates directory, waits until directory is available before returning. 
        /// </summary>
        /// <param name="dir"></param>
        /// <exception cref="Exception"></exception>
        public static void CreateDirectory(string dir, int maxAttempts = 100)
        {
            Directory.CreateDirectory(dir);

            int pause = 10;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                if (Directory.Exists(dir)) 
                {
                    if (attempts > 0) 
                    { 
                        ILog log = App.UnityContainer.Resolve<ILog>();
                        log.LogInfo($"Took {attempts} attempts to create directory {dir}");
                    }

                    break;
                }

                attempts++;

                Thread.Sleep(pause);
            }

            if (attempts == maxAttempts)
                throw new Exception($"Directory {dir} was created, but could not detect it after {attempts} attempts");
        }
    }
}
